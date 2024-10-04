using BuildingBlocks.Consul;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Yarp;
using MicroserviceDemo.APIGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseInfrastructure();
app.Run();