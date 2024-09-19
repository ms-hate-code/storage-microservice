using BuildingBlocks.EventStoreDB.Models;

namespace BuildingBlocks.EventStoreDB.Repository
{
    public interface IEventStoreDBRepository<T> where T : class, IAggregateEventSourcing<Guid>
    {
        Task<T> Find(Guid id, CancellationToken cancellationToken);
        Task<ulong> Add(T aggregate, CancellationToken cancellationToken);
        Task<ulong> Update(T aggregate, long? expectedRevision = null, CancellationToken cancellationToken = default);
        Task<ulong> Delete(T aggregate, long? expectedRevision = null, CancellationToken cancellationToken = default);
    }
}
