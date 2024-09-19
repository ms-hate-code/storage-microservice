using BuildingBlocks.PersistMessageStore.Data;
using BuildingBlocks.PersistMessageStore.Interfaces;
using BuildingBlocks.PersistMessageStore.Interfaces.Implements;
using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.PersistMessageStore
{
    public static class Extension
    {
        public static IServiceCollection AddPersistMessageProcessor(this IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddValidateOptions<PersistMessageOptions>();

            services.AddDbContext<PersistMessageDbContext>((builder, options) =>
            {
                var persistMessageOptions = builder.GetRequiredService<PersistMessageOptions>();

                options.UseNpgsql(
                    persistMessageOptions.ConnectionString,
                    dbOptions =>
                    {
                        dbOptions.MigrationsAssembly(typeof(PersistMessageDbContext).Assembly.GetName().Name);
                    }
                )
                .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IPersistMessageDbContext>(provider =>
            {
                var persistMessageDbContext = provider.GetRequiredService<PersistMessageDbContext>();

                persistMessageDbContext.Database.EnsureCreated();
                persistMessageDbContext.CreatePersistMessageTable();

                return persistMessageDbContext;
            });

            services.AddScoped<IPersistMessageProcessor, PersistMessageProcessor>();

            services.AddHostedService<PersistMessageBackgroundService>();

            return services;
        }
    }
}
