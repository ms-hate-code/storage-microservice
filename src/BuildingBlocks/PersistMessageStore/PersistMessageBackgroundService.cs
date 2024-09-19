using BuildingBlocks.PersistMessageStore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.PersistMessageStore
{
    public class PersistMessageBackgroundService
    (
        ILogger<PersistMessageBackgroundService> _logger,
        IOptions<PersistMessageOptions> _options,
        IServiceProvider _serviceProvider
    ) : BackgroundService
    {
        private Task _executingTask;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(PersistMessageBackgroundService)} Start");

            _executingTask = ProcessPersistMessageAsync(stoppingToken);

            return _executingTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(PersistMessageBackgroundService)} Stop");

            return base.StopAsync(cancellationToken);
        }

        private async Task ProcessPersistMessageAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var persistMessageProcessor = scope.ServiceProvider.GetRequiredService<IPersistMessageProcessor>();

                await persistMessageProcessor.ProcessAllAsync(cancellationToken);

                var intervalSecond = TimeSpan.FromSeconds(_options.Value.IntervalSecond);

                await Task.Delay(intervalSecond, cancellationToken);
            }
        }
    }
}
