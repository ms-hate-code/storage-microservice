namespace BuildingBlocks.Jwt
{
    public class JwtOptions
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string MetadataAddress { get; set; }
    }
}
