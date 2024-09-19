using BuildingBlocks.Utils;
using System.Collections.Concurrent;

namespace BuildingBlocks.EventStoreDB.Mappers
{
    public class EventTypeMapper
    {
        private static readonly EventTypeMapper _instance = new();
        private readonly ConcurrentDictionary<string, Type> typeMap = new();
        private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

        public static string ToName<TEventType>()
        {
            return ToName(typeof(TEventType));
        }

        public static string ToName(Type eventType)
        {
            return _instance.typeNameMap.GetOrAdd(eventType, _ =>
            {
                var eventTypeName = eventType.FullName!.Replace(".", "_");

                _instance.typeMap.AddOrUpdate(eventTypeName, eventType, (_, _) => eventType);

                return eventTypeName;
            });
        }

        public static Type ToType(string eventTypeName)
        {
            return _instance.typeMap.GetOrAdd(eventTypeName, _ =>
            {
                var type = TypeProvider.GetFirstMatchingTypeFromCurrentDomainAssembly(eventTypeName.Replace("_", "."));

                if (type is null)
                {
                    return null;
                }

                _instance.typeNameMap.AddOrUpdate(type, eventTypeName, (_, _) => eventTypeName);

                return type;
            });
        }
    }
}
