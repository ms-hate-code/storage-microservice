using Newtonsoft.Json;

namespace BuildingBlocks.EventStoreDB.Serialization
{
    public static class Extension
    {
        public static JsonSerializerSettings WithNonDefaultConstructorContractResolver(this JsonSerializerSettings settings)
        {
            settings.ContractResolver = new NonDefaultConstructorContractResolver();
            return settings;
        }

    }
}
