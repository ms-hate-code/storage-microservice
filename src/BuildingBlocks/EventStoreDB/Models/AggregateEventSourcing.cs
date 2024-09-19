using BuildingBlocks.Core.Event;
using BuildingBlocks.Core.Model;

namespace BuildingBlocks.EventStoreDB.Models
{
    public abstract class AggregateEventSourcing<TId> : Entity<TId>, IAggregateEventSourcing<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IDomainEvent[] ClearDomainEvents()
        {
            var domainEvents = _domainEvents.ToArray();

            _domainEvents.Clear();

            return domainEvents;
        }

        public virtual void When(object @event) { }
    }
}
