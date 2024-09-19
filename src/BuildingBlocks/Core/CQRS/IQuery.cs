using MediatR;

namespace BuildingBlocks.Core.CQRS
{
    public interface IQuery<TQuery> : IRequest<TQuery>
        where TQuery : notnull
    {
    }
}
