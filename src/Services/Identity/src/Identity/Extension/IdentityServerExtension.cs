using BuildingBlocks.Caching;
using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Identity.Configurations;
using Identity.Data;
using Identity.Identity.Constants;
using Identity.Identity.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Identity.Extension
{
    public static class IdentityServerExtension
    {
        public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
        {
            var authOptions = builder.Services.GetOptions<AuthOptions>(nameof(AuthOptions));
            builder.Services.AddValidateOptions<AuthOptions>();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var postgresOptions = builder.Services.GetOptions<PostgresOptions>(nameof(PostgresOptions));

            builder.Services
                .AddIdentity<AppUser, IdentityRole>(config =>
                {
                    config.Password.RequiredLength = 1;
                    config.Password.RequireDigit = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            var redisOptions = builder.Configuration.GetOptions<RedisOptions>(nameof(RedisOptions));

            var redisConnectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions.Host);

            builder.Services.AddTransient<ClientStore>();
            builder.Services.AddTransient<ResourceStore>();

            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = authOptions.IssuerUri;
                    options.Caching.ClientStoreExpiration = new TimeSpan(0, 0, 30);
                })
                .AddOperationalStore(options =>
                {
                    options.RedisConnectionMultiplexer = redisConnectionMultiplexer;
                    options.Db = redisOptions.DbNumber;
                    options.KeyPrefix = Constants.RedisKeyPrefix.OperationalStore;
                })
                .AddRedisCaching(options =>
                {
                    options.Db = redisOptions.DbNumber;
                    options.KeyPrefix = Constants.RedisKeyPrefix.ConfigureStore;
                    options.RedisConnectionMultiplexer = redisConnectionMultiplexer;
                })
                .AddClientStoreCache<ClientStore>()
                .AddResourceStoreCache<ResourceStore>()
                .AddAspNetIdentity<AppUser>()
                .AddResourceOwnerValidator<UserValidator>()
                .AddProfileServiceCache<ProfileService>()
                .AddDeveloperSigningCredential();

            builder.Services.AddScoped<IProfileService, ProfileService>();

            return builder;
        }
    }
}
