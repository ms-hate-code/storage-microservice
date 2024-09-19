using BuildingBlocks.Core.Event;
using BuildingBlocks.PersistMessageStore.Interfaces;
using MassTransit;

namespace BuildingBlocks.MassTransit
{
    public class ConsumeFilter<T>
    (
        IPersistMessageProcessor _persistMessageProcessor
    ) : IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var messageId = await _persistMessageProcessor.AddReceivedMessageAsync(
                new MessageEnvelope(
                    context.Message,
                    context.Headers.ToDictionary(x => x.Key, x => x.Value)
                )
            );

            var message = await _persistMessageProcessor.ExistProcessedInboxMessageAsync(messageId);

            if (message is null)
            {
                await next.Send(context);
                await _persistMessageProcessor.ProcessInboxAsync(messageId);
            }
        }
    }
}
