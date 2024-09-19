using System.Collections.Concurrent;

namespace BuildingBlocks.EventStoreDB.Mappers
{
    public class StreamMapper
    {
        private static readonly StreamMapper _instance = new();

        private readonly ConcurrentDictionary<Type, string> _typeNameMap = new();

        public static void AddCustomMap(Type streamType, string mappedStreamName)
        {
            _instance._typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
        }

        public static string ToStreamId<TStream>(object aggregateId, object tenantId = null)
        {
            return ToStreamId(typeof(TStream), aggregateId, tenantId);
        }
        
        public static string ToStreamId(Type streamType, object aggregateId, object tenantId = null)
        {
            var tenantPrefix = tenantId != null ? $"{tenantId}_" : "";

            return $"{tenantPrefix}{streamType.Name}-{aggregateId}";
        }
    }
}
