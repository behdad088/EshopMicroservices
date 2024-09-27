using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.HealthChecks;
using eshop.Shared;
using Order.Command.API.Configurations;
using Order.Command.Application;
using Order.Command.Infrastructure;
using Order.Command.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var databaseConfigurations = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultHealthChecks();
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(databaseConfigurations.SqlDatabase);

var app = builder.Build();
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialisedDatabaseAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api/v1/order/command/")
    .WithTags("Order Command API")
    .WithOpenApi();

app.UseProblemDetailsResponseExceptionHandler();
app.Run();