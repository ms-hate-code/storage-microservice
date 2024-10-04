using BuildingBlocks.Constants;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using FluentValidation;
using Mapster;
using MediatR;
using Metadata.Data;
using Metadata.FileObject.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metadata.FileObject.Features.Queries.GetFileObject.V1;

public record GetFileObjectQuery(Guid Id) : IQuery<GetFileObjectResult>
{
}

public record GetFileObjectResult
(
    string DisplayName,
    string FileUrl,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    string CreatedBy,
    string LastModifiedBy,
    string Folder
);

public record GetFileObjectResponseDto
(
    string DisplayName,
    string FileUrl,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    string CreatedBy,
    string LastModifiedBy,
    string Folder
);

public class GetFileObjectEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet($"{EndpointConfig.BaseAPIPath}/metadata/files/{{metadataId}}", async
                (
                    [FromRoute] Guid metadataId, 
                    IMediator _mediator
                ) =>
                {
                    var query = new GetFileObjectQuery(metadataId);

                    var result = await _mediator.Send(query);

                    var response = result.Adapt<GetFileObjectResponseDto>();

                    return Results.Ok(new APIResponse<GetFileObjectResponseDto>(StatusCodes.Status200OK, response));
                }
             )
            .RequireAuthorization(Common.AuthServer.STORAGE_APP)
            .WithName("GetFileObject")
            .WithApiVersionSet(builder.NewApiVersionSet("Metadata").Build())
            .Produces<GetFileObjectResponseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get File Object")
            .WithDescription("Get File Object")
            .WithOpenApi()
            .HasApiVersion(1.0);


        return builder;
    }
}

public class GetFileObjectValidator : AbstractValidator<GetFileObjectQuery>
{
    public GetFileObjectValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetFileObjectHandler
(
    ICurrentUserProvider _currentUserProvider,
    ILogger<GetFileObjectHandler> _logger,
    MetadataContext _context
)
    : IQueryHandler<GetFileObjectQuery, GetFileObjectResult>
{
    public async Task<GetFileObjectResult> Handle(GetFileObjectQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserProvider.GetUserId();
        var functionName = $"{nameof(GetFileObjectHandler)} UserId = {currentUserId} =>";
        try
        {
            _logger.LogInformation($"{functionName} Payload = {Helpers.JsonHelper.Serialize(request)}");

            var metadata = await _context.FileObjects
                .Include(x => x.FileFolder)
                .AsNoTracking()
                .Where(x => x.Id == request.Id && x.UserId == currentUserId)
                .Select(x => new GetFileObjectResult
                    (
                        x.DisplayFileName,
                        x.Url,
                        x.CreatedAt,
                        x.LastModifiedAt,
                        x.CreatedBy,
                        x.LastModifiedBy,
                        x.FileFolder.Name
                    )
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (metadata is null)
            {
                throw new FileObjectNotFoundException("Cannot find metadata of file");
            }

            return metadata;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{functionName} Message = {e.Message}");
            throw;
        }
    }
}