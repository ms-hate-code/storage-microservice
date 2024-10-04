using BuildingBlocks.Caching;
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

namespace Identity.Identity.Features.Queries.GetUserProfile.V1;

public record GetUserProfileQuery(string UserId) : IQuery<GetUserProfileResult>, ICachingRequest
{
    public string CacheKey { get; set; } = "IdentityService_CurrentUser";
    public string HashField { get; set; } = UserId;
    public TimeSpan ExpiredTime { get; set; }
}

public record GetUserProfileResult
(
    string Id,
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName
);

public record GetUserProfileResponseDto
(
    string Id,
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName
);

public class GetUserProfileEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet($"{EndpointConfig.BaseAPIPath}/current-user-profile", async
                (ICurrentUserProvider _currentUserProvider, IMediator _mediator) =>
            {
                var query = new GetUserProfileQuery(_currentUserProvider.GetUserId());

                var result = await _mediator.Send(query);

                var response = result.Adapt<GetUserProfileResponseDto>();

                return Results.Ok(new APIResponse<GetUserProfileResponseDto>(StatusCodes.Status200OK, response));
            })
            .RequireAuthorization(Common.AuthServer.STORAGE_BOTH_APP)
            .WithName("GetUserProfile")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<GetUserProfileResponseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get Current User")
            .WithDescription("Get Current User")
            .WithOpenApi()
            .HasApiVersion(1.0);


        return builder;
    }
}

public class GetUserProfileValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class GetCurrenUserQueryHandler
(
    UserManager<AppUser> _userManager,
    ILogger<GetCurrenUserQueryHandler> _logger
)
    : IQueryHandler<GetUserProfileQuery, GetUserProfileResult>
{
    public async Task<GetUserProfileResult> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        const string functionName = $"{nameof(GetCurrenUserQueryHandler)} =>";
        try
        {
            _logger.LogInformation($"{functionName} Payload = {Helpers.JsonHelper.Serialize(request)}");
                
            var user = await _userManager.FindByIdAsync(request.UserId)
                       ?? throw new UserNotFoundException("Cannot find current user");

            return user.Adapt<GetUserProfileResult>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{functionName} Message = {e.Message}");
            throw;
        }
    }
}