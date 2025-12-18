using Basket.API.ApiClient.AccessToken;
using Basket.API.ApiClient.OrderCommand;
using Basket.API.Authorization;
using Basket.API.Common;
using Basket.API.Features.CheckoutBasket.Commands;
using Discount;
using eshop.Shared;
using eshop.Shared.CQRS.Extensions;
using eshop.Shared.Exceptions.Handler;
using eshop.Shared.HealthChecks;
using eshop.Shared.Logger;
using eshop.Shared.Middlewares;
using eshop.Shared.OpenTelemetry;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.RegisterMediateR(typeof(Program).Assembly);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;

const string serviceName = "eshop.basket.api";
builder.Services.AddOpenTelemetryOtl(serviceName);

builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

builder.Services
    .AddOptions<DiscountGrpcConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

builder.Services
    .AddOptions<LoggerConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

builder.Services
    .AddOptions<OrderCommandClientConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var discountGrpcConfiguration = builder.Configuration.TryGetValidatedOptions<DiscountGrpcConfigurations>();
var databaseConfigurations = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();
var loggerConfigurations = builder.Configuration.TryGetValidatedOptions<LoggerConfigurations>();

builder.SetupLogging("Basket Service", environment, loggerConfigurations.ElasticSearch);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();


builder.Services.AddOrderCommandClient(builder.Configuration);
builder.Services.AddApiClientHandlers();

builder.Services.AddMarten(options =>
{
    options.Connection(databaseConfigurations.PostgresDb);
    options.Schema.For<ShoppingCart>()
        .UseNumericRevisions(true);

}).UseLightweightSessions();

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>((_, option) =>
{
    option.Address = new Uri(discountGrpcConfiguration.DiscountGrpc);
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = databaseConfigurations.Redis;
});
builder.Services.AddHealthChecks(builder.Configuration);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseTraceIdentifierHeader();
app.MapDefaultHealthChecks();
app.UseProblemDetailsResponseExceptionHandler();

app.MapGroup("/api/v1/basket")
    .WithTags("Basket API")
    .WithOpenApi()
    .RegisterEndpoints();


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


namespace Basket.API
{
    public class Program
    {
    }
}
