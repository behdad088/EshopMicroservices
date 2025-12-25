using eshop.Shared;
using eshop.Shared.Configurations;
using eshop.Shared.HealthChecks;
using eshop.Shared.Logger;
using eshop.Shared.OpenTelemetry;
using FluentValidation;
using Order.Query.EventProcessor;
using Order.Query.EventProcessor.Configurations;
using Order.Query.EventProcessor.Health;
using Order.Query.PostgresConfig;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.TrySetConfiguration<RabbitMqConfigurations>(builder.Configuration, out var rabbitMqConfigurations);
builder.Services.TrySetConfiguration<DatabaseConfigurations>(builder.Configuration, out var connectionString);
builder.Services.TrySetConfiguration<QueueConfigurations>(builder.Configuration, out var queues);
builder.Services.TrySetConfiguration<LoggerConfigurations>(builder.Configuration, out var loggerConfigurations);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
const string serviceName = "eshop.order.event.processor";
builder.Services.AddOpenTelemetryOtl(serviceName);
builder.SetupLogging("Order Event Processor", environment, loggerConfigurations.ElasticSearch);

builder.Services.AddPostgresDb(connectionString.PostgresDb);
builder.Services.AddApplicationServices(rabbitMqConfigurations, queues);
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHealthChecks(builder.Configuration);

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();

try
{
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e,"Unhandled Exception");
}
finally
{
    Log.Information("Log Complete");
    Log.CloseAndFlush();
}


namespace Order.Query.EventProcessor
{
    public class Program
    {
    }
}