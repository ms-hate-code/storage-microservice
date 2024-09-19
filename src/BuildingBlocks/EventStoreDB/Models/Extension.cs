using BuildingBlocks.EventStoreDB.Mappers;
using BuildingBlocks.EventStoreDB.Serialization;
using EventStore.Client;

namespace BuildingBlocks.EventStoreDB.Models
{
    public static class Extension
    {
        public static StreamEvent ToStreamEvent(this ResolvedEvent resolvedEvent)
        {
            var eventData = resolvedEvent.Deserialize();
            if (eventData is null)
            {
                return null;
            }

            var metadata = new EventData(resolvedEvent.Event.EventNumber.ToUInt64(), resolvedEvent.Event.Position.CommitPosition);
            var type = typeof(StreamEvent<>).MakeGenericType(eventData.GetType());

            return (StreamEvent)Activator.CreateInstance(type, eventData, metadata);
        }

        public static async Task<T> AggregateStream<T>
        (
            this EventStoreClient eventStoreClient,
            Guid id,
            CancellationToken cancellationToken,
            ulong? fromVersion = null
        ) where T : class, IProjection
        {
            var readResult = eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                StreamMapper.ToStreamId<T>(id),
                fromVersion ?? StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                return null;
            }

            var aggregate = (T)Activator.CreateInstance(typeof(T), true);

            await foreach (var @event in readResult)
            {
                var eventData = @event.Deserialize();

                aggregate.When(eventData);
            }

            return aggregate;
        }
    }
}
