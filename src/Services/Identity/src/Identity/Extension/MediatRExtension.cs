using BuildingBlocks.Logging;
using BuildingBlocks.Validation;
using Identity.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extension
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(IdentityRoot).Assembly);

                    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                }
            );

            return services;
        }
    }
}
