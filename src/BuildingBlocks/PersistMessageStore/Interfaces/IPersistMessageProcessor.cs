using BuildingBlocks.Core.Event;
using BuildingBlocks.PersistMessageStore.Model;
using System.Linq.Expressions;

namespace BuildingBlocks.PersistMessageStore.Interfaces
{
    public interface IPersistMessageProcessor
    {
        Task PublishMessageAsync<TMessageEnvelope>(
            TMessageEnvelope messageEnvelope,
            CancellationToken cancellationToken = default)
            where TMessageEnvelope : MessageEnvelope;

        Task<Guid> AddReceivedMessageAsync<TMessageEnvelope>(
            TMessageEnvelope messageEnvelope,
            CancellationToken cancellationToken = default)
            where TMessageEnvelope : MessageEnvelope;

        Task AddInternalMessageAsync<TCommand>(
            TCommand internalCommand,
            CancellationToken cancellationToken = default)
            where TCommand : class, IInternalCommand;

        Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
            Expression<Func<PersistMessage, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<PersistMessage> ExistProcessedInboxMessageAsync(
            Guid messageId,
            CancellationToken cancellationToken = default);

        Task ProcessInboxAsync(
            Guid messageId,
            CancellationToken cancellationToken = default);

        Task ProcessAsync(
            Guid messageId,
            MessageDeliveryType deliveryType,
            CancellationToken cancellationToken = default);

        Task ProcessAllAsync(CancellationToken cancellationToken = default);
    }
}
