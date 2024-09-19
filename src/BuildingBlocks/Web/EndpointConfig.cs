using Asp.Versioning.Builder;

namespace BuildingBlocks.Web
{
    public class EndpointConfig
    {
        public const string BaseAPIPath = "api/v{version:apiVersion}";
        public static ApiVersionSet ApiVersionSet { get; private set; } = default!;
    }
}
