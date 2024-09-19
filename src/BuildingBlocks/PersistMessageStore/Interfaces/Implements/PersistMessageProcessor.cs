using BuildingBlocks.Core.Event;
using BuildingBlocks.PersistMessageStore.Model;
using BuildingBlocks.Utils;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;

namespace BuildingBlocks.PersistMessageStore.Interfaces.Implements
{
    public class PersistMessageProcessor
    (
        IPersistMessageDbContext _persistMessageDbContext,
        ILogger<PersistMessageProcessor> _logger,
        IMediator _mediator,
        IPublishEndpoint _publishEndpoint
    ) : IPersistMessageProcessor
    {
        private readonly string _functionName = $"{nameof(PersistMessageProcessor)} =>";

        public async Task<Guid> AddReceivedMessageAsync<TMessageEnvelope>(
            TMessageEnvelope messageEnvelope, CancellationToken cancellationToken = default) where TMessageEnvelope : MessageEnvelope
        {
            return await SavePersistMessageAsync(messageEnvelope, MessageDeliveryType.INBOX, cancellationToken);
        }

        public async Task PublishMessageAsync<TMessageEnvelope>(
            TMessageEnvelope messageEnvelope, CancellationToken cancellationToken = default) where TMessageEnvelope : MessageEnvelope
        {
            var messageId = await SavePersistMessageAsync(messageEnvelope, MessageDeliveryType.OUTBOX, cancellationToken);
            await ProcessAsync(messageId, MessageDeliveryType.OUTBOX, cancellationToken);
        }

        public async Task AddInternalMessageAsync<TCommand>(
            TCommand internalCommand, CancellationToken cancellationToken) where TCommand : class, IInternalCommand
        {
            var messageId = await SavePersistMessageAsync(new MessageEnvelope(internalCommand, null), MessageDeliveryType.INTERNAL, cancellationToken);
            await ProcessAsync(messageId, MessageDeliveryType.INTERNAL, cancellationToken);
        }

        public async Task<PersistMessage> ExistProcessedInboxMessageAsync(
            Guid messageId, CancellationToken cancellationToken = default)
        {
            var persistMessage = await _persistMessageDbContext.PersistMessages
                .FirstOrDefaultAsync(x => x.Id == messageId
                    && x.MessageDeliveryType == MessageDeliveryType.INBOX
                    && x.MessageStatus == MessageStatus.PROCESSED
                    , cancellationToken);

            return persistMessage;
        }

        public async Task<IReadOnlyList<PersistMessage>> GetByFilterAsync(
            Expression<Func<PersistMessage, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var persistMessages = await _persistMessageDbContext.PersistMessages
                .Where(predicate)
                .ToListAsync(cancellationToken);

            return persistMessages.AsReadOnly();
        }

        public async Task ProcessAllAsync(CancellationToken cancellationToken = default)
        {
            var persistMessages = await _persistMessageDbContext.PersistMessages
                .Where(x => x.MessageStatus == MessageStatus.IN_PROGRESSING)
                .ToListAsync(cancellationToken);

            foreach (var persistMessage in persistMessages)
            {
                await ProcessAsync(persistMessage.Id, persistMessage.MessageDeliveryType, cancellationToken);
            }
        }

        public async Task ProcessAsync(
            Guid messageId, MessageDeliveryType deliveryType, CancellationToken cancellationToken = default)
        {
            var persistMessage = await _persistMessageDbContext.PersistMessages
                .FirstOrDefaultAsync(x => x.Id == messageId
                    && x.MessageDeliveryType == deliveryType,
                    cancellationToken);

            if (persistMessage == null)
            {
                _logger.LogWarning($"{_functionName} => Message id = {messageId} and Delivery type = {deliveryType} not found in persistence message store.");
                return;
            }

            switch (deliveryType)
            {
                case MessageDeliveryType.OUTBOX:
                    var sendOutboxMessage = await ProcessOutboxAsync(persistMessage, cancellationToken);
                    if (sendOutboxMessage)
                    {
                        await ChangeMessageStatusAsync(persistMessage, cancellationToken);
                    }
                    break;
                case MessageDeliveryType.INTERNAL:
                    var sendInternalMessage = await ProcessInternalAsync(persistMessage, cancellationToken);
                    if (sendInternalMessage)
                    {
                        await ChangeMessageStatusAsync(persistMessage, cancellationToken);
                    }
                    break;
                default:
                    break;
            }
        }

        public async Task ProcessInboxAsync(
            Guid messageId, CancellationToken cancellationToken = default)
        {
            var persistMessage = await _persistMessageDbContext.PersistMessages
                .FirstOrDefaultAsync(x => x.Id == messageId
                                   && x.MessageDeliveryType == MessageDeliveryType.INBOX
                                   && x.MessageStatus == MessageStatus.IN_PROGRESSING
                                   , cancellationToken);

            await ChangeMessageStatusAsync(persistMessage, cancellationToken);
        }

        private async Task<bool> ProcessOutboxAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageEnvelope = JsonSerializer.Deserialize<MessageEnvelope>(persistMessage.Data);

                if (messageEnvelope is null || messageEnvelope.Message is null)
                {
                    return false;
                }

                var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(persistMessage.DataType) ?? typeof(object);

                var data = JsonSerializer.Deserialize(messageEnvelope.Message.ToString(), type);

                if (data is not IEvent)
                {
                    return false;
                }

                await _publishEndpoint.Publish(data, context =>
                {
                    foreach (var header in messageEnvelope.Headers)
                        context.Headers.Set(header.Key, header.Value);
                }, cancellationToken);

                _logger.LogInformation(
                    $"{_functionName} => Message id = {persistMessage.Id} and delivery type = {persistMessage.MessageDeliveryType} processed from the persistence message store.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_functionName} => Error occurred while processing message id: {persistMessage.Id}, delivery type: {persistMessage.MessageDeliveryType}. Message = {ex.Message}");
                await ChangeMessageStatusAsync(persistMessage, cancellationToken, MessageStatus.IN_PROGRESSING, true);

                return false;
            }
        }

        private async Task<bool> ProcessInternalAsync(PersistMessage persistMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageEnvelope = JsonSerializer.Deserialize<MessageEnvelope>(persistMessage.Data);

                if (messageEnvelope is null || messageEnvelope.Message is null)
                {
                    return false;
                }

                var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(persistMessage.DataType) ?? typeof(object);

                var data = JsonSerializer.Deserialize(messageEnvelope.Message.ToString(), type);

                if (data is not IInternalCommand internalCommand)
                {
                    return false;
                }

                await _mediator.Send(internalCommand, cancellationToken);

                _logger.LogInformation(
                    $"InternalCommand with id: {persistMessage.Id} and delivery type: {persistMessage.MessageDeliveryType} processed from the persistence message store.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while processing message with id: {persistMessage.Id}, message type: {persistMessage.MessageDeliveryType}");
                await ChangeMessageStatusAsync(persistMessage, cancellationToken, MessageStatus.IN_PROGRESSING, true);

                return false;
            }
        }

        private async Task<Guid> SavePersistMessageAsync(
                MessageEnvelope messageEnvelope,
                MessageDeliveryType deliveryType,
                CancellationToken cancellationToken = default
            )
        {
            Guid messageId = Guid.NewGuid();
            if (messageEnvelope.Message is IEvent message)
            {
                messageId = message.EventId;
            }

            var messageExists = await _persistMessageDbContext.PersistMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (messageExists != null)
            {
                return messageId;
            }

            var persistMessage = new PersistMessage(
                messageId,
                messageEnvelope.Message.GetType().ToString(),
                JsonSerializer.Serialize(messageEnvelope),
                deliveryType
            );

            await _persistMessageDbContext.PersistMessages.AddAsync(persistMessage, cancellationToken);

            await _persistMessageDbContext.SaveChangesAsync(cancellationToken);


            _logger.LogInformation($"Message with id: {messageId} and delivery type: {deliveryType} saved in persistence message store.");

            return messageId;
        }

        private async Task ChangeMessageStatusAsync(
            PersistMessage message,
            CancellationToken cancellationToken,
            MessageStatus messageStatus = MessageStatus.PROCESSED,
            bool isProcessFailed = false)
        {
            if (message is null)
            {
                return;
            }

            message.ChangeState(messageStatus);

            if (isProcessFailed)
            {
                message.IncreaseRetry();
            }

            _persistMessageDbContext.PersistMessages.Update(message);

            await _persistMessageDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
