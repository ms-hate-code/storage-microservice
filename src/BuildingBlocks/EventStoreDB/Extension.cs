using BuildingBlocks.EventStoreDB.BackgroundWorkers;
using BuildingBlocks.EventStoreDB.Projection;
using BuildingBlocks.EventStoreDB.Repository;
using BuildingBlocks.EventStoreDB.Subscriptions;
using BuildingBlocks.Web;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BuildingBlocks.EventStoreDB
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; } = default!;
    }

    public record EventStoreDBOptions(
        bool UseInternalCheckpointing = true
    );

    public static class Extension
    {
        public static IServiceCollection AddEventStore
        (
            this IServiceCollection services,
            params Assembly[] assemblies
        )
        {
            services.AddValidateOptions<EventStoreOptions>();

            var assembliesToScan = assemblies.Length > 0 ? assemblies : [Assembly.GetEntryAssembly()];

            return services
                .AddEventStoreDB()
                .AddProjections(assembliesToScan);
        }

        public static IServiceCollection AddEventStoreDBSubscriptionToAll
        (
            this IServiceCollection services,
            EventStoreDBSubscriptionToAllOptions subscriptionOptions = null,
            bool checkpointToEventStoreDB = true
        )
        {
            if (checkpointToEventStoreDB)
                services.AddTransient<ISubscriptionCheckpointRepository, SubscriptionCheckpointRepository>();

            return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var eventStoreDBSubscriptionToAll =
                    serviceProvider.GetRequiredService<EventStoreDBSubscriptionToAll>();

                return new BackgroundWorker(
                    logger,
                    ct => eventStoreDBSubscriptionToAll.SubscribeToAll(
                            subscriptionOptions ?? new EventStoreDBSubscriptionToAllOptions(),
                            ct
                        )
                );
            }
            );
        }
        private static IServiceCollection AddEventStoreDB(this IServiceCollection services, EventStoreDBOptions options = default)
        {

            services
                .AddSingleton(x =>
                {
                    var eventStoreOptions = services.GetOptions<EventStoreOptions>(nameof(EventStoreOptions));
                    return new EventStoreClient(EventStoreClientSettings.Create(eventStoreOptions.ConnectionString));
                })
                .AddScoped(typeof(IEventStoreDBRepository<>), typeof(EventStoreDBRepository<>))
                .AddTransient<EventStoreDBSubscriptionToAll, EventStoreDBSubscriptionToAll>();

            if (options?.UseInternalCheckpointing == true)
                services.AddTransient<ISubscriptionCheckpointRepository, SubscriptionCheckpointRepository>();

            return services;
        }


        private static IServiceCollection AddProjections(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped<IProjectionPublisher, ProjectionPublisher>();
            RegisterProjections(services, assemblies);

            return services;
        }

        private static void RegisterProjections(IServiceCollection services, Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<IProjectionProcessor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }
}
