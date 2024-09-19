namespace BuildingBlocks.HealthCheck
{
    public class HealthCheckOptions
    {
        public bool Enabled { get; set; } = true;
        public int TimeIntervalChecking { get; set; } = 60;
    }
}
