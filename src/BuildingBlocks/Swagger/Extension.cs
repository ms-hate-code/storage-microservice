using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Swagger
{
    public static class Extension
    {

        private static readonly OpenApiSecurityScheme scheme = new()
        {
            Description = @"JWT authorization header using the Bearer sheme. \r\n\r\n
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "ouath2",
        };

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
                {
                    options.OperationFilter<SwaggerDefaultValues>();

                    options.AddSecurityDefinition("Bearer", scheme);
                    options.OperationFilter<SwaggerSecurityRequirements>();

                    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                }
            );

            return services;
        }
        public static IApplicationBuilder UseCustomSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    var descriptions = app.DescribeApiVersions();

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }
                });

            return app;
        }
    }
}
