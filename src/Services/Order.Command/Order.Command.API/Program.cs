using System.Reflection;
using eshop.Shared;
using eshop.Shared.Exceptions.Handler;
using eshop.Shared.HealthChecks;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;
using Microsoft.AspNetCore.Authorization;
using Order.Command.API.Common;
using Order.Command.API.Configurations;
using Order.Command.Application;
using Order.Command.Infrastructure;
using Order.Command.Infrastructure.Data.Extensions;
using Order.Command.Application.Configurations;
using Order.Command.Application.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOptions<LoggerConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var loggerConfigurations = builder.Configuration.TryGetValidatedOptions<LoggerConfigurations>();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
const string serviceName = "eshop.order.command.api";
builder.Services.AddOpenTelemetryOtl(serviceName);
builder.SetupLogging("Order Command Service", environment, loggerConfigurations.ElasticSearch);


builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var databaseConfigurations = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();

builder.Services
    .AddOptions<RabbitMqConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var rabbitMqConfigurations = builder.Configuration.TryGetValidatedOptions<RabbitMqConfigurations>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks(databaseConfigurations.SqlDatabase);

builder.Services.AddScoped<IUser, CurrentUser>();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddApplicationServices(rabbitMqConfigurations)
    .AddInfrastructureServices(databaseConfigurations.SqlDatabase);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
builder.AddDefaultAuthentication(Policies.ConfigureAuthorization);

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});
app.UseTraceIdentifierHeader();
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialisedDatabaseAsync();
}

var routeGroupBuilder = app.MapGroup("/api/v1/")
    .WithTags("Order Command API")
    .WithOpenApi();

app.MapEndpoints(routeGroupBuilder);

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


namespace Order.Command.API
{
    public class Program
    {
        
    }
}
