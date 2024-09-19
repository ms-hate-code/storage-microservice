using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web
{
    public static class ApiVersioningExtension
    {
        public static void AddCustomAPIVersioning(this IServiceCollection services,
            Action<ApiVersioningOptions>? configurator = null)
        {
            services.AddApiVersioning(options =>
            {
                // return api-version in response header
                options.ReportApiVersions = true;
                // set the default version when the client has not specified any versions
                options.AssumeDefaultVersionWhenUnspecified = true;
                // set the default version to 1.0
                options.DefaultApiVersion = new ApiVersion(1, 0);
                // defind how the version is read from the request
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version"),
                    new QueryStringApiVersionReader("api-version")
                );

                configurator?.Invoke(options);
            })
            .AddApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            })
            .AddMvc();
        }
    }
}
