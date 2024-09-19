using BuildingBlocks.Core.Event;

namespace BuildingBlocks.Core
{
    public interface IEventMapper
    {
        IIntegrationEvent? MapDomainEventToIntegrationEvent<TDomainEvent>(TDomainEvent @event);
        IInternalCommand? MapDomainEventToInternalCommand<TDomainEvent>(TDomainEvent @event);
    }
}
