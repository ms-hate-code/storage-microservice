using FluentValidation;

namespace BuildingBlocks.Validation
{
    public static class Extension
    {
        public static async Task HandleValidationAsync<TRequest>(this IValidator<TRequest> validator, TRequest request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            ValidationResponse responseValidation = new(result.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList());

            if (!result.IsValid)
            {
                throw new Exceptions.ValidationException(responseValidation.ToString());
            }
        }
    }
}
