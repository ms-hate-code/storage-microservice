using BuildingBlocks.Consul;
using BuildingBlocks.EFCore;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using FluentValidation;
using Identity.Configurations;
using Identity.Data;
using Identity.Data.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Identity.Extension
{
    public static class InfrastructureExtension
    {
        public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var environment = builder.Environment;

            builder.AddCustomSerilog(environment);

            builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddConsul();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
            builder.Services.AddCustomDbContext<IdentityContext>();
            builder.Services.AddScoped<IDataSeeder, IdentityDataSeeder>();
            builder.Services.AddCustomSwagger();
            builder.Services.AddCustomAPIVersioning();
            builder.Services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
            builder.Services.AddCustomMediatR();
            builder.Services.AddProblemDetails();
            builder.Services.AddCustomMapster(typeof(IdentityRoot).Assembly);
            builder.Services.AddCustomHealthCheck();

            builder.AddCustomIdentityServer();
            builder.Services.AddJwt();

            var authOptions = builder.Services.GetOptions<AuthOptions>(nameof(AuthOptions));
            builder.Services.AddHttpClient(authOptions.ClientId, c =>
                {
                    c.BaseAddress = new Uri(authOptions.IssuerUri);
                }
            );
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            return builder;
        }

        public static WebApplication UseInfrastructure(this WebApplication app)
        {
            var env = app.Environment;
            var appOptions = app.GetOptions<AppOptions>(nameof(AppOptions));

            app.UseCustomProblemDetails();
            app.UseForwardedHeaders();
            app.UseCustomHealthCheck();

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = LogEnrichHelper.EnrichFromRequest;
            });
            app.UseMigration<IdentityContext>(env);
            app.UseCorrelationId();
            app.UseIdentityServer();
            app.MapControllers();

            app.MapGet("/api/healths", () => Results.Ok());

            app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

            if (env.IsDevelopment())
            {
                app.UseCustomSwagger();
            }

            return app;
        }
    }
}
