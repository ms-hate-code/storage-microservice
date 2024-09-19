using MediatR;

namespace BuildingBlocks.Core.CQRS
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TResponse : notnull
        where TQuery : IQuery<TResponse>
    {
    }
}
