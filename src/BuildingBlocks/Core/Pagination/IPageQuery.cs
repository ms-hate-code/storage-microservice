using MediatR;

namespace BuildingBlocks.Core.Pagination
{
    public interface IPageQuery<TResponse> : IPageRequest, IRequest<TResponse>
        where TResponse : class
    {
    }
}
