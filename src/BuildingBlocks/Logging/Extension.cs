using BuildingBlocks.Constants;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SpectreConsole;
using System.Text;

namespace BuildingBlocks.Logging
{
    public static class Extension
    {
        public static WebApplicationBuilder AddCustomSerilog(this WebApplicationBuilder builder, IWebHostEnvironment env)
        {
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                var environment = Environment.GetEnvironmentVariable(AppConstants.ASPNETCORE_ENVIRONMENT);
                var appOptions = context.Configuration.GetOptions<AppOptions>(nameof(AppOptions));
                var logOptions = context.Configuration.GetOptions<LogOptions>(nameof(LogOptions));

                var logLevel = Enum.TryParse<LogEventLevel>(logOptions.Level, true, out var level)
                    ? level
                    : LogEventLevel.Information;

                loggerConfiguration
                    .MinimumLevel.Is(logLevel)
                    .WriteTo.SpectreConsole(logOptions.LogTemplate, logLevel)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    // Only show ef-core information in error level
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                    // Filter out ASP.NET Core infrastructure logs that are Information and below
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration);

                if (logOptions.Elastic.Enabled)
                {
                    loggerConfiguration.WriteTo.Elasticsearch(
                        new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(logOptions.Elastic.ElasticServiceUrl))
                        {
                            AutoRegisterTemplate = true,
                            IndexFormat = logOptions.Elastic.IndexFormat ?? $"{appOptions.Name}-{environment?.ToLower()}",
                        }
                    );
                }

                if (logOptions.File.Enabled)
                {
                    var root = env.ContentRootPath;
                    Directory.CreateDirectory(Path.Combine(root, "logs"));

                    var path = string.IsNullOrWhiteSpace(logOptions.File.Path) ? "logs/.txt" : logOptions.File.Path;

                    if (!Enum.TryParse<RollingInterval>(logOptions.File.Interval, true, out var interval))
                    {
                        interval = RollingInterval.Day;
                    }

                    loggerConfiguration.WriteTo.File(
                         path: path,
                         rollingInterval: interval,
                         encoding: Encoding.UTF8,
                         outputTemplate: logOptions.LogTemplate
                    );
                }
            });

            return builder;
        }
    }
}
