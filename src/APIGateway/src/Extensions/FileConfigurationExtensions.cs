using Ocelot.Configuration.File;

namespace MicroserviceDemo.APIGateway.Extensions;

public static class FileConfigurationExtensions
{
    public static IServiceCollection ConfigureOcelotServiceDiscoveryProvider(this IServiceCollection services)
    {
        services.PostConfigure<FileConfiguration>(fileConfiguration =>
        {
            var serviceDiscoveryProvider = fileConfiguration.GlobalConfiguration.ServiceDiscoveryProvider;
            var host = Environment.GetEnvironmentVariable("OCELOT_SERVICE_DISCOVERY_HOST") ??
                       throw new InvalidOperationException();

            serviceDiscoveryProvider.Host = host;
        });

        return services;
    }
}