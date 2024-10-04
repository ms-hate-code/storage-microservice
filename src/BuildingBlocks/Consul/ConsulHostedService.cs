using BuildingBlocks.Web;
using Consul;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Consul
{
    public class ConsulHostedService
    (
        IConsulClient _client, 
        ConsulOptions _config, 
        IHostApplicationLifetime _lifetime
    )
        : IHostedService
    {
        private AgentServiceRegistration _registration;

        // Registers service to Consul registry
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(async () => await OnStarted(cancellationToken));
            _lifetime.ApplicationStopping.Register(async () => await OnStopping(cancellationToken));
            _lifetime.ApplicationStopped.Register(async () => await OnStopped(cancellationToken));
        }

        // If the service is shutting down it deregisters service from Consul registry
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);
        }
        
        private async Task OnStarted(CancellationToken cancellationToken)
        {
            var url = GetCurrentHost();
            Console.WriteLine(url);
            _registration = new AgentServiceRegistration
            {
                ID = _config.Name + $"_{Guid.NewGuid()}",
                Name = _config.Name,
                Address = url.Host,
                Port = url.Port,
                Check = new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(20),
                    HTTP = $"{url.Scheme}://{url.Host}:{url.Port}/{_config.HealthCheckEndPoint}",
                    Timeout = TimeSpan.FromSeconds(5),
                    TLSSkipVerify = true
                }
            };

            // Deregister already registered service
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);

            // Registers service
            await _client.Agent.ServiceRegister(_registration, cancellationToken).ConfigureAwait(false);
        }
        
        private async Task OnStopping(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnStopped(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);
        }
        
        private static Uri GetCurrentHost()
        {
            var iPAddress = GlobalExtension.GetCurrentHost();
            return new Uri(iPAddress);
        }
    }
}
