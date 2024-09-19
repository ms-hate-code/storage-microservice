using BuildingBlocks.Core.Event;
using BuildingBlocks.PersistMessageStore.Interfaces;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuildingBlocks.Core
{
    public class EventDispatcher(
        IEventMapper _eventMapper,
        ILogger<EventDispatcher> _logger,
        IServiceScopeFactory _serviceScopeFactory,
        IHttpContextAccessor _httpContextAccessor,
        IPersistMessageProcessor _persistMessageProcessor
    ) : IEventDispatcher
    {

        public async Task DispatchAsync<T>(T @event, Type type, CancellationToken cancellationToken)
            where T : IEvent
        {
            await DispatchAsync([@event], type, cancellationToken);
        }

        public async Task DispatchAsync<T>(IReadOnlyList<T> events, Type type, CancellationToken cancellationToken)
            where T : IEvent
        {
            if (events == null || events.Count == 0)
            {
                await Task.CompletedTask;
            }

            var eventType = type != null && type.IsAssignableTo(typeof(IInternalCommand))
                ? EventType.INTERNAL_COMMAND
                : EventType.DOMAIN_EVENT;


            async Task PublishIntegrationEvent(IReadOnlyList<IIntegrationEvent> integrationEvents)
            {
                foreach (var integrationEvent in integrationEvents)
                {
                    // publish message to persist storage
                    await _persistMessageProcessor.PublishMessageAsync(
                        new MessageEnvelope(integrationEvent, SetHeaders())
                        , cancellationToken);
                }
            }

            switch (events)
            {
                case IReadOnlyList<IDomainEvent> domainEvents:
                    {
                        var integrationEvents = await MapDomainEventToIntegrationEvent(domainEvents);

                        await PublishIntegrationEvent(integrationEvents);
                        break;
                    }
                case IReadOnlyList<IIntegrationEvent> integrationEvents:
                    {
                        await PublishIntegrationEvent(integrationEvents);
                        break;
                    }
            }

            if (type != null && eventType == EventType.INTERNAL_COMMAND)
            {
                var internalCommands = await MapDomainEventToInternalCommand(events as IReadOnlyList<IDomainEvent>);

                foreach (var internalCommand in internalCommands)
                {
                    // publish internal message to persist storage
                    await _persistMessageProcessor.AddInternalMessageAsync(internalCommand, cancellationToken);
                }
            }
        }

        #region Private Methods

        private Task<IReadOnlyList<IIntegrationEvent>> MapDomainEventToIntegrationEvent(IReadOnlyList<IDomainEvent> events)
        {
            _logger.LogTrace("Processing integration events start...");

            var integrationEvents = new List<IIntegrationEvent>();
            using var scope = _serviceScopeFactory.CreateScope();
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                _logger.LogTrace($"Handling domain event: {eventType.Name}");

                var integrationEvent = _eventMapper.MapDomainEventToIntegrationEvent(@event);

                if (integrationEvent is null) continue;

                integrationEvents.Add(integrationEvent);
            }

            _logger.LogTrace("Processing integration events done...");

            return Task.FromResult<IReadOnlyList<IIntegrationEvent>>(integrationEvents);
        }

        private Task<IReadOnlyList<IInternalCommand>> MapDomainEventToInternalCommand(IReadOnlyList<IDomainEvent> events)
        {
            _logger.LogTrace("Processing internal message start...");

            var integrationEvents = new List<IInternalCommand>();
            using var scope = _serviceScopeFactory.CreateScope();
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                _logger.LogTrace($"Handling domain event: {eventType.Name}");

                var integrationEvent = _eventMapper.MapDomainEventToInternalCommand(@event);

                if (integrationEvent is null) continue;

                integrationEvents.Add(integrationEvent);
            }

            _logger.LogTrace("Processing internal message done...");

            return Task.FromResult<IReadOnlyList<IInternalCommand>>(integrationEvents);
        }

        private Dictionary<string, object> SetHeaders()
        {
            var headers = new Dictionary<string, object>();

            var httpContext = _httpContextAccessor.HttpContext;
            headers.Add("CorrelationId", httpContext.GetCorrelationId());
            headers.Add("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            headers.Add("Email", httpContext.User.FindFirstValue(ClaimTypes.Email));

            return headers;
        }

        #endregion
    }
}
