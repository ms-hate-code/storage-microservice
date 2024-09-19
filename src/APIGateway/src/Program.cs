using BuildingBlocks.Consul;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Yarp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConsul();
builder.Services.AddYarpWithConsul(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("_allowOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.AddCustomHealthCheck();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("_allowOrigins");

app.UseCustomHealthCheck();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.MapGet("/api/healths", () => Results.Ok());

app.Run();