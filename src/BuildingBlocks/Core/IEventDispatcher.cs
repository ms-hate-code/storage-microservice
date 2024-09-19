using BuildingBlocks.Core.Event;

namespace BuildingBlocks.Core
{
    public interface IEventDispatcher
    {
        Task DispatchAsync<T>(T @event, Type type, CancellationToken cancellationToken)
            where T : IEvent;
        Task DispatchAsync<T>(IReadOnlyList<T> @events, Type type, CancellationToken cancellationToken)
            where T : IEvent;
    }
}
