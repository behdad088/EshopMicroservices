using System.Reflection;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.HealthChecks;
using eshop.Shared;
using Order.Command.API.Common;
using Order.Command.API.Configurations;
using Order.Command.API.Endpoints.CreateOrder;
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
builder.Services.AddDefaultHealthChecks();
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(databaseConfigurations.SqlDatabase);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialisedDatabaseAsync();
}

var routeGroupBuilder = app.MapGroup("/api/v1/order/command/")
    .WithTags("Order Command API")
    .WithOpenApi();

app.MapEndpoints(routeGroupBuilder);

app.UseProblemDetailsResponseExceptionHandler();
app.Run();