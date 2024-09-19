using BuildingBlocks.EventStoreDB.Mappers;
using BuildingBlocks.EventStoreDB.Models;
using BuildingBlocks.EventStoreDB.Serialization;
using EventStore.Client;

namespace BuildingBlocks.EventStoreDB.Repository
{
    public class EventStoreDBRepository<T> 
    (
        EventStoreClient _eventStoreClient
    ): IEventStoreDBRepository<T> where T : class, IAggregateEventSourcing<Guid>
    {

        public async Task<ulong> Add(T aggregate, CancellationToken cancellationToken)
        {
            var result = await _eventStoreClient.AppendToStreamAsync(
                StreamMapper.ToStreamId<T>(aggregate.Id),
                StreamState.NoStream,
                GetEventsToStore(aggregate), 
                cancellationToken: cancellationToken
            );

            return result.NextExpectedStreamRevision;
        }

        public async Task<ulong> Delete(T aggregate, long? expectedRevision = null, CancellationToken cancellationToken = default)
        {
            return await Update(aggregate, expectedRevision, cancellationToken);
        }

        public async Task<T> Find(Guid id, CancellationToken cancellationToken)
        {
            return await _eventStoreClient.AggregateStream<T>(id, cancellationToken);
        }

        public async Task<ulong> Update(T aggregate, long? expectedRevision = null, CancellationToken cancellationToken = default)
        {
            var nextVersion = expectedRevision ?? aggregate.Version;
            var result = await _eventStoreClient.AppendToStreamAsync(
                StreamMapper.ToStreamId<T>(aggregate.Id),
                (ulong)nextVersion,
                GetEventsToStore(aggregate),
                cancellationToken: cancellationToken
            );

            return result.NextExpectedStreamRevision;
        }

        private static IEnumerable<EventStore.Client.EventData> GetEventsToStore(T aggregate)
        {
            var events = aggregate.ClearDomainEvents();

            return events
                .Select(EventStoreDBSerializer.ToJsonEventData);
        }
    }
}
