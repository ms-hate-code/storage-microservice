using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace BuildingBlocks.EventStoreDB.BackgroundWorkers
{
    public class BackgroundWorker
    (
        ILogger<BackgroundWorker> _logger, 
        Func<CancellationToken, Task> _performMethod
    ) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () => 
            {
                await Task.Yield();
                _logger.LogInformation("Background worker started");
                await _performMethod(stoppingToken);
                _logger.LogInformation("Background worker stopped");
            }, stoppingToken);
        }
    }
}
