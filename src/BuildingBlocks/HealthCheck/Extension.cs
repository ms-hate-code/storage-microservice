using BuildingBlocks.EFCore;
using BuildingBlocks.Logging;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Web;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.HealthCheck
{
    public static class Extension
    {
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services)
        {
            var healthcheckOptions = services.GetOptions<HealthCheckOptions>(nameof(HealthCheckOptions));

            if (!healthcheckOptions.Enabled)
            {
                return services;
            }

            var appOptions = services.GetOptions<AppOptions>(nameof(AppOptions));
            var postgresOptions = services.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            var rabbitMqOptions = services.GetOptions<RabbitMQOptions>(nameof(RabbitMQOptions));
            var logOptions = services.GetOptions<LogOptions>(nameof(LogOptions));

            var healthCheckServiceBuilder = services.AddHealthChecks();

            if (!string.IsNullOrEmpty(rabbitMqOptions.Host))
            {
                healthCheckServiceBuilder
                    .AddRabbitMQ(
                        rabbitConnectionString:
                        $"amqp://{rabbitMqOptions?.UserName}:{rabbitMqOptions?.Password}@{rabbitMqOptions?.Host}");
            }

            if (!string.IsNullOrEmpty(postgresOptions.ConnectionString))
            {
                healthCheckServiceBuilder
                    .AddNpgSql(postgresOptions?.ConnectionString);
            }

            if (!string.IsNullOrEmpty(logOptions?.Elastic?.ElasticServiceUrl))
            {
                healthCheckServiceBuilder
                    .AddElasticsearch(logOptions?.Elastic?.ElasticServiceUrl);
            }

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(healthcheckOptions.TimeIntervalChecking);
                setup.AddHealthCheckEndpoint($"Health Checking - {appOptions.Name}", "/healthz");
            }).AddInMemoryStorage();

            return services;
        }

        public static WebApplication UseCustomHealthCheck(this WebApplication app)
        {
            var healthcheckOptions = app.GetOptions<HealthCheckOptions>(nameof(HealthCheckOptions));

            if (!healthcheckOptions.Enabled)
            {
                return app;
            }

            app
                .UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    AllowCachingResponses = false,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }

                })
                .UseHealthChecksUI(options =>
                {
                    options.ApiPath = "/healthcheck";
                    options.UIPath = "/healthcheck-ui";
                });

            return app;
        }
    }

}
