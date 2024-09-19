using Consul;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using YarpDestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using YarpRouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;

namespace BuildingBlocks.Yarp
{
    public class YarpProxyConfigProvider : IProxyConfigProvider, IDisposable
    {
        private readonly InMemoryConfigProvider _inMemoryConfigProvider = new(Array.Empty<YarpRouteConfig>(), Array.Empty<ClusterConfig>());
        private readonly CancellationTokenSource _stoppingToken = new();

        private readonly IEnumerable<IProxyConfigProvider> _providers;
        private readonly IConsulClient _consulClient;
        private readonly IOptionsMonitor<YarpServiceDiscoveryOptions> _options;

        public YarpProxyConfigProvider(IEnumerable<IProxyConfigProvider> providers,
            IConsulClient consulClient, IOptionsMonitor<YarpServiceDiscoveryOptions> options)
        {
            _providers = providers;
            _consulClient = consulClient;
            _options = options;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            IntervalUpdate(_stoppingToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public IProxyConfig GetConfig() => _inMemoryConfigProvider.GetConfig();

        private async Task IntervalUpdate(CancellationToken cancellationToken)
        {
            try
            {
                do
                {
                    await UpdateConfig(cancellationToken);

                    var interval = _options.CurrentValue.UpdateIntervalSeconds;
                    await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);

                } while (!cancellationToken.IsCancellationRequested);
            }
            catch
            {

            }
        }

        private async Task UpdateConfig(CancellationToken cancellationToken)
        {
            List<ClusterConfig> clusterConfigs = new();
            List<YarpRouteConfig> routes = new();

            foreach (IProxyConfigProvider provider in _providers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                IProxyConfig config = provider.GetConfig();

                routes.AddRange(config.Routes);
                clusterConfigs.AddRange(await CreateClusters(config, cancellationToken));
            }

            _inMemoryConfigProvider.Update(routes, clusterConfigs);
        }

        private async Task<List<ClusterConfig>> CreateClusters(IProxyConfig proxyConfig, CancellationToken cancellationToken)
        {
            Dictionary<string, AgentService> consulServices = (await _consulClient.Agent.Services(cancellationToken)).Response;

            List<ClusterConfig> clusters = new();

            foreach (ClusterConfig cluster in proxyConfig.Clusters)
            {
                Dictionary<string, YarpDestinationConfig> destination = CreateDestinationConfigForCluster(cluster, consulServices);

                ClusterConfig newCluster = cluster with
                {
                    Destinations = destination
                };

                clusters.Add(newCluster);
            }

            return clusters;
        }

        private Dictionary<string, YarpDestinationConfig> CreateDestinationConfigForCluster(
            ClusterConfig defaultClusterConfig,
            Dictionary<string, AgentService> discoveryServices
        )
        {
            Dictionary<string, YarpDestinationConfig> destinations = new();
            if (defaultClusterConfig is null)
            {
                return destinations;
            }

            foreach (KeyValuePair<string, YarpDestinationConfig> destination in defaultClusterConfig.Destinations)
            {
                string serviceName = destination.Value.Address;

                AgentService service = discoveryServices.Values
                                .FirstOrDefault(s => s.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

                if (service is null)
                {
                    continue;
                }

                string destinationName = service.ID;

                bool hasHealthCheck = service.Meta.TryGetValue("HealthCheckEndPoint", out string healthCheckEndpoint);
                bool hasScheme = service.Meta.TryGetValue("Scheme", out string scheme);

                if (!hasScheme)
                {
                    continue;
                }

                string address = $"{scheme}://{service.Address}:{service.Port}";

                destinations.Add(destinationName, new()
                {
                    Address = address,
                    Health = healthCheckEndpoint,
                    Metadata = destination.Value.Metadata
                });
            }

            return destinations;
        }

        public void Dispose()
        {
            _stoppingToken.Cancel();
            _stoppingToken.Dispose();
        }
    }
}
