using BuildingBlocks.Consul;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;  
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Mvc;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Serilog;

namespace MicroserviceDemo.APIGateway.Extensions;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var routes = "Routes";
        builder.Configuration.AddOcelotWithSwaggerSupport(options =>
        {
            options.Folder = routes;
        });
        
        builder.Services
            .AddSwaggerForOcelot(builder.Configuration)
            .AddOcelot(builder.Configuration)
            .AddPolly()
            .AddCacheManager(x =>
            {
                x.WithDictionaryHandle();
            })
            .AddConsul()
            .AddConfigStoredInConsul();
        builder.Services.ConfigureOcelotServiceDiscoveryProvider();

        var configPath = Path.Combine(builder.Environment.ContentRootPath, routes);
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("ocelot.json")
            .AddOcelot(configPath, builder.Environment)
            .AddEnvironmentVariables();
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        builder.Services
            .AddJwt()
            .AddConsul()
            .AddProblemDetails()
            .AddHttpContextAccessor()
            .AddCustomSwagger()
            .AddCustomAPIVersioning()
            .AddProblemDetails()
            .AddCustomHealthCheck();
        
        // builder.Services.AddCustomOpenTelemetry(builder.Configuration);

        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        var env = app.Environment;
        var appOptions = app.GetOptions<AppOptions>(nameof(AppOptions));

        if (env.IsDevelopment())
        {
            app.UseCustomSwagger();
        }
        
        app.UseCors();
        app.UseRouting();
        app.UseHttpsRedirection();
        
        app.UseCustomProblemDetails();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseForwardedHeaders();
        app.UseCustomHealthCheck();
        
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = LogEnrichHelper.EnrichFromRequest;
        });
        app.UseCorrelationId();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSwaggerForOcelotUI(options =>
            {
                options.PathToSwaggerGenerator = "/swagger/docs";
            })
            .UseOcelot()
            .Wait();
        
        app.MapGet("/api/healths", () => Results.Ok());
        app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

        return app;
    }
}