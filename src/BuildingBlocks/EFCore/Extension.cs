using BuildingBlocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EFCore
{
    public static class Extension
    {
        public static IServiceCollection AddCustomDbContext<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            // When this setting is enabled, Npgsql will treat timestamp values as UTC and adjust them for the local time zone
            // when reading and writing to the database
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var postgresOptions = services.GetOptions<PostgresOptions>(nameof(PostgresOptions));

            services.AddDbContext<TContext>((sp, options) =>
            {

                options.UseNpgsql(
                    postgresOptions?.ConnectionString,
                    dbOptions =>
                    {
                        dbOptions.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name);
                    }
                )
                .UseSnakeCaseNamingConvention();
            });

            return services;
        }

        public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app, IWebHostEnvironment env)
            where TContext : DbContext
        {
            MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();
            //SeedDataAsync(app.ApplicationServices).GetAwaiter().GetResult();

            return app;
        }

        private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
            where TContext : DbContext
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.MigrateAsync();
        }

        private static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            foreach (var seeder in seeders)
            {
                await seeder.SeedAllAsync();
            }
        }
    }
}
