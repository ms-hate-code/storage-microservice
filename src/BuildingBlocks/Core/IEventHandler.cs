using BuildingBlocks.Core.Event;
using MediatR;

namespace BuildingBlocks.Core
{
    public interface IEventHandler<TEvent> : INotificationHandler<TEvent>
        where TEvent : IEvent
    {
    }
}
