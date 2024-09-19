using BuildingBlocks.EventStoreDB.Mappers;
using BuildingBlocks.EventStoreDB.Models;
using BuildingBlocks.EventStoreDB.Projection;
using BuildingBlocks.Utils;
using EventStore.Client;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.EventStoreDB.Subscriptions
{
    public class EventStoreDBSubscriptionToAll
    (
        IProjectionPublisher _projectionPublisher,
        EventStoreClient _eventStoreClient,
        ISubscriptionCheckpointRepository _subscriptionCheckpointRepository,
        ILogger<EventStoreDBSubscriptionToAll> _logger,
        IMediator _mediator
    )
    {
        private string functionName = $"{nameof(EventStoreDBSubscriptionToAll)} {nameof(SubscribeToAll)} =>";
        private EventStoreDBSubscriptionToAllOptions subscriptionOptions = default;
        private string subscriptionId => subscriptionOptions.SubscriptionId;
        private CancellationToken cancellationToken = default;
        private readonly object resubscribeLock = new();

        public async Task SubscribeToAll(EventStoreDBSubscriptionToAllOptions options, CancellationToken ct)
        {
            await Task.Yield();

            subscriptionOptions = options;
            cancellationToken = ct;

            var checkpoint = await _subscriptionCheckpointRepository.Load(subscriptionId, ct);

            _logger.LogInformation($"{functionName} Subscribing to all '{subscriptionOptions.SubscriptionId}' is starting");

            await _eventStoreClient.SubscribeToAllAsync(
                checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
                HandleEvent,
                subscriptionOptions.ResolveLinkTos,
                HandleDropEvent,
                subscriptionOptions.FilterOptions,
                subscriptionOptions.Credentials,
                ct
             );

            _logger.LogInformation($"{functionName} Subscription to all '{subscriptionOptions.SubscriptionId}' is started");
        }

        private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken ct) 
        {
            try
            {
                if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

                var streamEvent = resolvedEvent.ToStreamEvent();

                if (streamEvent is null)
                {
                    _logger.LogWarning($"{functionName} {nameof(HandleEvent)} Couldn't deserialize event with id: {resolvedEvent.Event.EventId}");
                    if (!subscriptionOptions.IgnoreDeserializationErrors)
                        throw new InvalidOperationException($"{functionName} {nameof(HandleEvent)} Unable to deserialize event {resolvedEvent.Event.EventType} with id: {resolvedEvent.Event.EventId}");

                    return;
                }

                // publish event to internal event bus (notification handler)
                await _mediator.Publish(streamEvent, ct);

                await _projectionPublisher.PublishAsync(streamEvent, ct);

                await _subscriptionCheckpointRepository.Store(subscriptionId, resolvedEvent.Event.Position.CommitPosition, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{functionName} Has error: Message = {ex.Message}");
                throw;
            }
        }

        private void HandleDropEvent(StreamSubscription _, SubscriptionDroppedReason reason, Exception exception)
        {
            _logger.LogError(exception, $"{functionName} {nameof(HandleDropEvent)} Subscription to all '{subscriptionId}' dropped with reason: '{reason}'");

            if (exception is RpcException { StatusCode: StatusCode.Cancelled })
                return;

            Resubscribe();
        }

        private void Resubscribe()
        {
            while (true)
            {
                var resubscribed = false;
                try
                {
                    Monitor.Enter(resubscribeLock);

                    using (NoSynchronizationContextScope.Enter())
                    {
                        SubscribeToAll(subscriptionOptions, cancellationToken).Wait(cancellationToken);
                    }

                    resubscribed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"{functionName} {nameof(Resubscribe)} Has error. Message = '{ex.Message}'");
                }
                finally
                {
                    Monitor.Exit(resubscribeLock);
                }

                if (resubscribed)
                    break;

                // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
                // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
                Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
            }
        }

        private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.Data.Length != 0) return false;

            _logger.LogInformation($"{functionName} Event without data");
            return true;
        }

        private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event.EventType != EventTypeMapper.ToName<CheckpointStored>()) return false;

            _logger.LogInformation($"{functionName} Checkpoint event - ignoring");
            return true;
        }
    }
}
