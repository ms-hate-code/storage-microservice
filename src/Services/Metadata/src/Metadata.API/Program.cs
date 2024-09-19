using BuildingBlocks.Web;
using Metadata;
using Metadata.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddMinimalEndpoints(assemblies: typeof(MetadataRoot).Assembly);
builder.AddInfrastructure();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseInfrastructure();
app.MapMinimalEndpoints();
app.Run();