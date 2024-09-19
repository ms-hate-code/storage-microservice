using MediatR;

namespace BuildingBlocks.Core.CQRS
{
    public interface ICommand<TCommand> : IRequest<TCommand>
        where TCommand : notnull
    {
    }
    public interface ICommand : ICommand<Unit>
    {
    }
}
