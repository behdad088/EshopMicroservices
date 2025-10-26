using BuildingBlocks.HealthChecks;
using eshop.Shared;
using FluentValidation;
using Order.Query.EventProcessor;
using Order.Query.EventProcessor.Configurations;
using Order.Query.EventProcessor.Health;
using Order.Query.PostgresConfig;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();
builder.Services
    .AddOptions<RabbitMqConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();
builder.Services
    .AddOptions<QueueConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var rabbitMqConfigurations = builder.Configuration.TryGetValidatedOptions<RabbitMqConfigurations>();
var connectionString = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();
var queues = builder.Configuration.TryGetValidatedOptions<QueueConfigurations>();

builder.Services.AddPostgresDb(connectionString.PostgresDb);

builder.Services.AddApplicationServices(rabbitMqConfigurations, queues);
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHealthChecks(builder.Configuration);


var app = builder.Build();
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

namespace Order.Query.EventProcessor
{
    public class Program
    {
    }
}