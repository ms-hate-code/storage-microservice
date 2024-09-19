using BuildingBlocks.Core.Event;

namespace BuildingBlocks.EventStoreDB.Models
{
    public record EventData(
        ulong StreamRevision,
        ulong LogPosition
    );

    public class StreamEvent(object data, EventData metadata) : IEvent
    {
        public object Data { get; } = data;
        public EventData Metadata { get; } = metadata;
    }

    public class StreamEvent<T>(T data, EventData metadata) : StreamEvent(data, metadata)
    {
        public new T Data => (T)base.Data;
    }
}
