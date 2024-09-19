using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Logging
{
    public class LoggingBehavior<TRequest, TResponse>
    (
        ILogger<LoggingBehavior<TRequest, TResponse>> _logger
    )
     : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var prefix = nameof(LoggingBehavior<TRequest, TResponse>);

            _logger.LogInformation($"[{prefix}] Handle request={typeof(TRequest).Name} and response={typeof(TResponse).Name}");

            var timer = new Stopwatch();
            timer.Start();

            var response = await next();

            timer.Stop();
            _logger.LogInformation($"[{prefix}] Handle request={typeof(TRequest).Name} executed in {timer.ElapsedMilliseconds} ms");

            if (timer.Elapsed.Seconds > 3)
            {
                _logger.LogWarning($"[{prefix}] Handle request={typeof(TRequest).Name} took {timer.Elapsed.Seconds} seconds");
            }

            _logger.LogInformation($"[{prefix}] Handled request={typeof(TRequest).Name}");

            return response;
        }
    }
}
