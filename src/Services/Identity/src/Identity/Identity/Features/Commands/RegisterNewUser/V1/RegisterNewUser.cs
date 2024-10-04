using BuildingBlocks.Constants;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Commands.RegisterNewUser.V1;

public record RegisterNewUserCommand
(
    RegisterNewUserRequestDto Payload
) : ICommand<RegisterNewUserResult>;

public record RegisterNewUserResult
(
    string Id,
    string Email,
    string FirstName,
    string LastName
);

public record RegisterNewUserRequestDto
(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName
);

public record RegisterNewUserResponseDto
(
    string Id,
    string Email,
    string FirstName,
    string LastName
);

public class RegisterNewUserValidator : AbstractValidator<RegisterNewUserCommand>
{
    public RegisterNewUserValidator()
    {
        RuleFor(x => x.Payload.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Payload.ConfirmPassword).Equal(x => x.Payload.Password);
        RuleFor(x => x.Payload.FirstName).NotEmpty();
        RuleFor(x => x.Payload.LastName).NotEmpty();
    }
}

public class RegisterNewUserEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseAPIPath}/register-user",
                async (RegisterNewUserRequestDto request, IMediator _mediator) =>
                {
                    var command = new RegisterNewUserCommand(request);

                    var result = await _mediator.Send(command);

                    var response = result.Adapt<RegisterNewUserResponseDto>();

                    return Results.Ok(new APIResponse<RegisterNewUserResponseDto>(StatusCodes.Status200OK, response));
                }
            )
            .WithName("RegisterUser")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<RegisterNewUserResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register new User")
            .WithDescription("Register new user")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}

public class RegisterNewUserCommandHandler
(
    UserManager<AppUser> _userManager,
    ILogger<RegisterNewUserCommandHandler> _logger
)
    : ICommandHandler<RegisterNewUserCommand, RegisterNewUserResult>
{
    public async Task<RegisterNewUserResult> Handle(RegisterNewUserCommand command, CancellationToken cancellationToken)
    {
        const string functionName = $"{nameof(RegisterNewUserCommandHandler)} =>";
        try
        {
            var payload = command.Payload;
            _logger.LogInformation($"{functionName} Payload = {Helpers.JsonHelper.Serialize(payload)}");
            var user = command.Adapt<AppUser>();
            user.UserName = payload.Email.Replace("@", "");
            var result = await _userManager.CreateAsync(user, payload.Password);

            if (!result.Succeeded)
            {
                throw new RegisterUserException(string.Join(',', result.Errors.Select(e => e.Description)));
            }
            var roleResult = await _userManager.AddToRoleAsync(user, Common.SystemRole.USER);

            if (!roleResult.Succeeded)
            {
                throw new RegisterUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
            }

            return user.Adapt<RegisterNewUserResult>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{functionName} Message = {e.Message}");
            throw;
        }
    }
}