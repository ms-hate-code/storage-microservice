using BuildingBlocks.Core.Event;
using BuildingBlocks.EventStoreDB.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EventStoreDB.Projection
{
    public class ProjectionPublisher
    (
        IServiceProvider _serviceProvider
    ) : IProjectionPublisher
    {

        public async Task PublishAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
             where T : IEvent
        {
            using var scope = _serviceProvider.CreateScope();
            var projectionProcessors = scope.ServiceProvider.GetRequiredService<IEnumerable<IProjectionProcessor>>();

            foreach (var projectionProcessor in projectionProcessors)
            {
                await projectionProcessor.ProcessEventAsync(streamEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task PublishAsync(StreamEvent streamEvent, CancellationToken cancellationToken = default)
        {
            var streamDataType = streamEvent.Data.GetType();

            var publishMethod = streamDataType
                    .GetMethods()
                    .Single(x => x.Name == nameof(PublishAsync) && x.GetGenericArguments().Length != 0)
                    .MakeGenericMethod(streamDataType);

            return publishMethod
                .Invoke(this, [streamEvent, cancellationToken]) as Task;
        }
    }
}
