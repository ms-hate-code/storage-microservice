using BuildingBlocks.Core.Event;
using BuildingBlocks.EventStoreDB.Models;

namespace BuildingBlocks.EventStoreDB.Projection
{
    public interface IProjectionProcessor
    {
        Task ProcessEventAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
            where T : IEvent;    
    }
}
