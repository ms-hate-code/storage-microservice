using BuildingBlocks.Web;
using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Consul
{
    public static class Extension
    {
        public static void AddConsul(this IServiceCollection services)
        {
            var consulOptions = services.GetOptions<ConsulOptions>(nameof(ConsulOptions))
                ?? throw new ArgumentNullException("Consul options are not configured");

            services.AddSingleton(consulOptions);

            var consulClient = new ConsulClient(config =>
            {
                config.Address = consulOptions.DiscoveryAddress;
            });

            services.AddSingleton<IConsulClient, ConsulClient>(_ => consulClient);
            services.AddSingleton<IHostedService, ServiceDiscoveryRegistrationHostedService>();

        }
    }
}
