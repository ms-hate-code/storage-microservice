using BuildingBlocks.Logging;
using BuildingBlocks.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Metadata.Extension
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(MetadataRoot).Assembly);

                    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                }
            );

            return services;
        }
    }
}
