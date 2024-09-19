using BuildingBlocks.Core.Event;

namespace BuildingBlocks.Core.Model
{
    public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IEvent[] ClearDomainEvents()
        {
            var events = _domainEvents.ToArray();

            _domainEvents.Clear();

            return events;
        }
    }
}
