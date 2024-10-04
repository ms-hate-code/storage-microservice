namespace BuildingBlocks.Consul
{
    public class ConsulOptions
    {
        public string Name { get; set; }
        public Uri DiscoveryAddress { get; set; }
        public string HealthCheckEndPoint { get; set; }
    }
}
