using MediatR;

namespace BuildingBlocks.Core.Event
{
    public interface IEvent : INotification
    {
        Guid EventId => Guid.NewGuid();
        DateTime OccurredOn => DateTime.UtcNow;
        string EventType => GetType().AssemblyQualifiedName;
    }
}
