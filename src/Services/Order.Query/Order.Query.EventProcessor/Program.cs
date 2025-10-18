using BuildingBlocks.HealthChecks;
using eshop.Shared;
using FluentValidation;
using Marten;
using Order.Query.Data.Events;
using Order.Query.Data.Views.OrderView;
using Order.Query.EventProcessor;
using Order.Query.EventProcessor.Configurations;
using Order.Query.EventProcessor.Health;

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

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString.PostgresDb);
    options.UseSystemTextJsonForSerialization(); 
    options.Schema.For<OrderView>()
        .Index(x => x.CustomerId)
        .Index(x => x.OrderStatus)
        .Index(x => x.TotalPrice)
        .Index(x => x.CreatedAt)
        .FullTextIndex(x => x.OrderName!)
        .UniqueIndex(x => x.Id);
    
    options.Schema.For<EventStream>()
        .Index(x => x.Id)
        .Index(x => x.ViewId)
        .Index(x => x.EventType)
        .Index(x => x.CreatedAt)
        .UniqueIndex(x => x.Id);
}).UseLightweightSessions();
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