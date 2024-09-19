using MediatR;

namespace BuildingBlocks.Core.CQRS
{
    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : notnull
    {
    }

    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand
    {
    }
}
