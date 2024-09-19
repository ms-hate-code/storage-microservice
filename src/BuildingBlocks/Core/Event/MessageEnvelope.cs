namespace BuildingBlocks.Core.Event
{
    public class MessageEnvelope
    (
        object? message,
        IDictionary<string, object>? headers
    )
    {
        public object? Message { get; init; } = message;
        public IDictionary<string, object>? Headers { get; init; } = headers;
    }

    public class MessageEnvelope<TMessage>
    (
        TMessage message,
        IDictionary<string, object> headers
    ) : MessageEnvelope(message, headers) where TMessage : class
    {
        public new TMessage? Message { get; init; } = message;
    }
}
