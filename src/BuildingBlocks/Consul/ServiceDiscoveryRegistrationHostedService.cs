using BuildingBlocks.Constants;
using BuildingBlocks.Web;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Consul
{
    public class ServiceDiscoveryRegistrationHostedService(
        IConsulClient _client,
        ILogger<ServiceDiscoveryRegistrationHostedService> _logger,
        IConfiguration _configuration,
        ConsulOptions _options
    ) : IHostedService
    {
        private AgentServiceRegistration _registration;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Uri appUrl = GetAppUrl();
            _registration = new AgentServiceRegistration
            {
                ID = _options.Id,
                Name = _options.Name,
                Address = _options.Address,
                Port = appUrl.Port,
                Check = new AgentServiceCheck()
                {
                    Interval = TimeSpan.FromSeconds(15),
                    HTTP = $"{appUrl.Scheme}://{_options.Address}:{appUrl.Port}/{_options.HealthCheckEndPoint}",
                    Timeout = TimeSpan.FromSeconds(5)
                },
                Meta = new Dictionary<string, string>
                {
                    { "Scheme", appUrl.Scheme },
                    { "HealthCheckEndPoint", _options.HealthCheckEndPoint }
                }
            };

            // Deregister already registered service
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);

            // Registers service
            await _client.Agent.ServiceRegister(_registration, cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);
        }

        private Uri GetAppUrl()
        {
            var appAddress = _configuration.GetOptions(AppConstants.ASPNETCORE_URLS);

            if (appAddress.Contains(";"))
            {
                var urls = appAddress.Split(";");
                appAddress = urls.FirstOrDefault(x => !x.Contains("https"));
            }

            var address = new Uri(appAddress);

            return address;
        }
    }
}
