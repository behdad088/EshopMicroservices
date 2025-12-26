using eshop.Shared;
using eshop.Shared.Configurations;
using eshop.Shared.Exceptions.Handler;
using eshop.Shared.Logger;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;
using Order.Query.API.Configurations;
using Order.Query.PostgresConfig;

var builder = WebApplication.CreateBuilder(args);

builder.Services.TrySetConfiguration<LoggerConfigurations>(builder.Configuration, out var loggerConfigurations);
builder.Services.TrySetConfiguration<DatabaseConfigurations>(builder.Configuration, out var connectionString);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
const string serviceName = "eshop.order.query.api";
builder.Services.AddOpenTelemetryOtl(serviceName);
builder.SetupLogging("Order Query Service", environment, loggerConfigurations.ElasticSearch);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();
builder.Services.AddPostgresDb(connectionString.PostgresDb);

builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
builder.AddDefaultAuthentication(Policies.ConfigureAuthorization);

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseTraceIdentifierHeader();
app.UseHttpsRedirection();
app.UseProblemDetailsResponseExceptionHandler();

app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
    c.Versioning.DefaultVersion = 1;
    c.Serializer.Options.PropertyNamingPolicy = null;
    c.Serializer.Options.AllowOutOfOrderMetadataProperties = true;
});

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

namespace Order.Query.Api
{
    public class Program
    {
    }
}