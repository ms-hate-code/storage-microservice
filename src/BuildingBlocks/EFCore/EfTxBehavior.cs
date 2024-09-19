using BuildingBlocks.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.EFCore
{
    public class EfTxBehavior<TRequest, TResponse>
    (
        ILogger<EfTxBehavior<TRequest, TResponse>> _logger,
        IDbContext _dbContext,
        IEventDispatcher _eventDispatcher
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var functionName = $"{typeof(EfTxBehavior<TRequest, TResponse>).Name} {nameof(Handle)} =>";
            _logger.LogInformation(
                $"{functionName} Handled command {typeof(TRequest).FullName}, content = {JsonSerializer.Serialize(request)}");

            var response = await next();

            var domainEvents = _dbContext.GetDomainEvents();

            if (domainEvents is null || !domainEvents.Any())
            {
                return response;
            }

            await _eventDispatcher.DispatchAsync(domainEvents.ToArray(), typeof(TRequest), cancellationToken);

            return response;
        }
    }
}
