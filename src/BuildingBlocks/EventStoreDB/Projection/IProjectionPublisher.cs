using BuildingBlocks.Core.Event;
using BuildingBlocks.EventStoreDB.Models;

namespace BuildingBlocks.EventStoreDB.Projection
{
    public interface IProjectionPublisher
    {
        Task PublishAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
             where T : IEvent;
        Task PublishAsync(StreamEvent streamEvent, CancellationToken cancellationToken = default);
    }
}
