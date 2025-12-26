using Catalog.API.Authorization;
using Catalog.API.Common;
using Catalog.API.Configurations.ConfigurationOptions;
using Catalog.API.Data;
using eshop.Shared;
using eshop.Shared.Configurations;
using eshop.Shared.CQRS.Extensions;
using eshop.Shared.Exceptions.Handler;
using eshop.Shared.HealthChecks;
using eshop.Shared.Logger;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.TrySetConfiguration<LoggerConfigurations>(builder.Configuration, out var loggerConfigurations);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
const string serviceName = "eshop.catalog.api";
builder.Services.AddOpenTelemetryOtl(serviceName);
builder.SetupLogging("Catalog Service", environment, loggerConfigurations.ElasticSearch);

builder.AddDefaultOpenApi();

builder.Services.RegisterMediateR(typeof(Program).Assembly);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
    options.Schema.For<ProductDocument>().UseNumericRevisions(true);
}).UseLightweightSessions();

builder.Services.AddHealthChecks(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialDataMigration>();
}

builder.AddDefaultAuthentication(Policies.ConfigureAuthorization);

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});
app.UseTraceIdentifierHeader();
app.UseDefaultOpenApi();
app.MapDefaultHealthChecks();

app.MapGroup("/api/v1/catalog")
    .WithTags("Catalog API")
    .RegisterEndpoints();
app.UseProblemDetailsResponseExceptionHandler();


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

namespace Catalog.API
{
    public class Program
    {
    }
}