using BuildingBlocks.Core.Event;
using BuildingBlocks.EventStoreDB.Serialization;
using EventStore.Client;

namespace BuildingBlocks.EventStoreDB.Subscriptions
{
    public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckpointedAt) : IEvent;

    public class SubscriptionCheckpointRepository 
    (
        EventStoreClient _eventStoreClient
    ) : ISubscriptionCheckpointRepository
    {
        public async Task<ulong?> Load(string subscriptionId, CancellationToken cancellationToken)
        {
            var checkpointStreamName = GetCheckpointStreamName(subscriptionId);

            var result = _eventStoreClient.ReadStreamAsync(
                Direction.Backwards, checkpointStreamName,
                StreamPosition.End, 
                cancellationToken: cancellationToken
            );

            if (await result.ReadState == ReadState.StreamNotFound)
            {
                return null;
            }

            ResolvedEvent? @event = await result.FirstOrDefaultAsync(cancellationToken);

            return @event?.Deserialize<CheckpointStored>()?.Position;
        }

        public async Task Store(string subscriptionId, ulong position, CancellationToken cancellationToken)
        {
            var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);
            var eventToAppend = new[] { @event.ToJsonEventData() };
            var checkpointStreamName = GetCheckpointStreamName(subscriptionId);

            try
            {
                // Store new checkpoint to exists stream 
                await _eventStoreClient.AppendToStreamAsync(
                    checkpointStreamName,
                    StreamState.StreamExists,
                    eventToAppend,
                    cancellationToken: cancellationToken
                );
            }
            catch (WrongExpectedVersionException)
            {
                // Stream did not exist
                // Set the checkpoint stream to have at most 1 event
                // using stream metadata with maxCount property
                await _eventStoreClient.SetStreamMetadataAsync(
                    checkpointStreamName,
                    StreamState.NoStream,
                    new StreamMetadata(1),
                    cancellationToken: cancellationToken
                );

                // Append event again to not exists stream
                await _eventStoreClient.AppendToStreamAsync(
                    checkpointStreamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: cancellationToken
                );
            }
        }

        private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
    }
}
