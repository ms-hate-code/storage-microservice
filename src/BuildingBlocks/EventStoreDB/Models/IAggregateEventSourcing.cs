using BuildingBlocks.Core.Event;
using BuildingBlocks.Core.Model;

namespace BuildingBlocks.EventStoreDB.Models
{
    public interface IAggregateEventSourcing<T> : IAggregateEventSourcing, IEntity<T>
    {
    }

    public interface IAggregateEventSourcing : IProjection, IEntity
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        IDomainEvent[] ClearDomainEvents();
    }
}
