namespace BuildingBlocks.MassTransit
{
    public class RabbitMQOptions
    {
        public string Host { get; set; }
        public string ExchangeName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ushort? Port { get; set; }
    }
}
