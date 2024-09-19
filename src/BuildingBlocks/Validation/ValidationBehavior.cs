using FluentValidation;
using MediatR;

namespace BuildingBlocks.Validation
{
    public class ValidationBehavior<TRequest, TResponse>
    (
        IValidator<TRequest> _validator
    ) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validator is null)
            {
                return await next();
            }

            await _validator.HandleValidationAsync(request, cancellationToken);

            return await next();
        }
    }
}
