using eshop.Shared;
using eshop.Shared.Logger;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;
using Identity.API;
using Identity.API.Configurations.ConfigurationOptions;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddOptions<LoggerConfigurations>()
        .Bind(builder.Configuration)
        .ValidateDataAnnotationsRecursively()
        .ValidateOnStart();

    var loggerConfigurations = builder.Configuration.TryGetValidatedOptions<LoggerConfigurations>();
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
    const string serviceName = "eshop.identity.api";
    builder.Services.AddOpenTelemetryOtl(serviceName);
    builder.SetupLogging("Identity Service", environment, loggerConfigurations.ElasticSearch);

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();
    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });
    app.UseTraceIdentifierHeader();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}