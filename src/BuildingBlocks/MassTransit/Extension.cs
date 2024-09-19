using BuildingBlocks.Exceptions;
using BuildingBlocks.Web;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.MassTransit
{
    public static class Extension
    {
        public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, Assembly assembly)
        {
            services.AddValidateOptions<RabbitMQOptions>();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(assembly);
                x.AddSagaStateMachines(assembly);
                x.AddSagas(assembly);
                x.AddActivities(assembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = services.GetOptions<RabbitMQOptions>(nameof(RabbitMQOptions));

                    cfg.Host(rabbitMqOptions.Host, "/", _ =>
                    {
                        _.Username(rabbitMqOptions.UserName);
                        _.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);

                    cfg.UseMessageRetry(_ =>
                    {
                        _.Exponential(
                            3,
                            TimeSpan.FromMilliseconds(200),
                            TimeSpan.FromMinutes(120),
                            TimeSpan.FromMilliseconds(200)
                        )
                        .Ignore<ValidationException>();
                    });
                });
            });

            return services;
        }
    }
}
