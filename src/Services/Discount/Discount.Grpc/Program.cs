using Discount.Grpc.Configurations.ConfigurationOptions;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using eshop.Shared;
using eshop.Shared.Configurations;
using eshop.Shared.Logger;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.TrySetConfiguration<DatabaseConfigurations>(builder.Configuration, out var databaseConfigurations);
builder.Services.TrySetConfiguration<LoggerConfigurations>(builder.Configuration, out var loggerConfigurations);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
const string serviceName = "eshop.discount.grpc";
builder.Services.AddOpenTelemetryOtl(serviceName);
builder.SetupLogging("Discount Service", environment, loggerConfigurations.ElasticSearch);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(options =>
    options.UseSqlite(databaseConfigurations.SqliteDb));

builder.Services.MigrateDatabase();
var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});
app.UseTraceIdentifierHeader();
// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

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


namespace Discount.Grpc
{
    public class Program
    {
    }
}