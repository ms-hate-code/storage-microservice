using BuildingBlocks.EventStoreDB.Mappers;
using EventStore.Client;
using Newtonsoft.Json;
using System.Text;

namespace BuildingBlocks.EventStoreDB.Serialization
{
    public static class EventStoreDBSerializer
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
            .WithNonDefaultConstructorContractResolver();

        public static T Deserialize<T>(this ResolvedEvent resolvedEvent) where T : class
        {
            return Deserialize(resolvedEvent) as T;
        }

        public static object Deserialize(this ResolvedEvent resolvedEvent)
        {
            var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

            if(eventType == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), 
                eventType, 
                _jsonSerializerSettings);
        }

        public static EventData ToJsonEventData(this object @event)
        {
            return new EventData(
                Uuid.NewUuid(), 
                EventTypeMapper.ToName(@event.GetType()), 
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { })
            ));
        }
    }
}
