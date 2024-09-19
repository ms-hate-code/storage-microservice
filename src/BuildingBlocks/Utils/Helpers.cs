using System.Text.Json;
using Newtonsoft.Json;

namespace BuildingBlocks.Utils;

public static class Helpers
{
    public static class JsonHelper
    {
        public static string Serialize(object data)
        {
            if (data is null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
            });
        }
    }
}