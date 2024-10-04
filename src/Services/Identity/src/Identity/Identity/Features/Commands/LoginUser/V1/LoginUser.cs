using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Jwt;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Configurations;
using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using IdentityModel.Client;
using IdentityServer4.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Identity.Features.Commands.LoginUser.V1;

public record LoginUserCommand
(
    LoginUserRequestDto Payload
) : ICommand<LoginUserResult>;

public record LoginUserResult
(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string Scope
);

public record LoginUserRequestDto
(
    string Email,
    string Password
);

public record LoginUserResponseDto
(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string Scope
);

public class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Payload.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Payload.Password).NotEmpty();
    }
}

public class LoginUserEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseAPIPath}/login",
                async (LoginUserRequestDto request, IMediator _mediator) =>
                {
                    var command = new LoginUserCommand(request);

                    var result = await _mediator.Send(command);

                    var response = result.Adapt<LoginUserResponseDto>();

                    return Results.Ok(APIResponse<LoginUserResponseDto>.Ok(response));
                }
            )
            .WithName("LoginUser")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<LoginUserResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Login User")
            .WithDescription("Login user")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}

public class LoginUserCommandHandler
(
    SignInManager<AppUser> _signInManager,
    UserManager<AppUser> _userManager,
    IHttpClientFactory _httpClientFactory,
    IOptions<AuthOptions> _authOptions,
    IOptions<JwtOptions> _jwtOptions,
    ILogger<LoginUserCommandHandler> _logger
)
    : ICommandHandler<LoginUserCommand, LoginUserResult>
{
    public async Task<LoginUserResult> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        const string functionName = $"{nameof(LoginUserCommandHandler)} =>";
        try
        {
            var payload = command.Payload;
            _logger.LogInformation($"{functionName} Payload = {Helpers.JsonHelper.Serialize(payload)}");
            var user = await _userManager.FindByEmailAsync(payload.Email)
                       ?? throw new InvalidCredentialsException("Email is incorrect");

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, payload.Password, false);

            if (!signInResult.Succeeded)
            {
                throw new InvalidCredentialsException("Password is incorrect");
            }

            var authValue = _authOptions.Value;

            var client = _httpClientFactory.CreateClient(authValue.ClientId);

            var disco = await client.GetDiscoveryDocumentAsync(cancellationToken: cancellationToken, request: new DiscoveryDocumentRequest()
            {
                Address = _jwtOptions.Value.MetadataAddress,
            });

            var tokenRequest = new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                GrantType = GrantType.ResourceOwnerPassword,
                UserName = payload.Email,
                Password = payload.Password,
                ClientId = authValue.ClientId,
                ClientSecret = authValue.ClientSecret,
                Scope = $"offline_access {authValue.Scope}"
            };

            var response = await client.RequestPasswordTokenAsync(tokenRequest, cancellationToken);

            if (response.IsError)
            {
                throw new LoginUserException(response.Error);
            }

            return new LoginUserResult(
                response.AccessToken,
                response.RefreshToken,
                response.ExpiresIn,
                response.Scope
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{functionName} Message = {e.Message}");
            throw;
        }
    }
}