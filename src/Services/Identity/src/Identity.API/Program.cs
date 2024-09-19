using BuildingBlocks.Constants;
using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Web;
using Identity;
using Identity.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddMinimalEndpoints(assemblies: typeof(IdentityRoot).Assembly);
builder.AddInfrastructure();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseInfrastructure();
app.MapMinimalEndpoints();

app.MapGet("/api/identity/test", x => x.Response.WriteAsJsonAsync(new APIResponse<string>(200, "Test identity")))
    .RequireAuthorization(Common.AuthServer.STORAGE_APP);

app.MapGet("/api/identity/exception", x => throw new Exception("Test"));

app.Run();
