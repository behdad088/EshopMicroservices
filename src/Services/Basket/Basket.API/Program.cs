using Basket.API.Authorization;
using Basket.API.Common;
using BuildingBlocks.HealthChecks;
using Discount;
using eshop.Shared;
using eshop.Shared.CQRS.Extensions;
using eshop.Shared.Exceptions.Handler;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.RegisterMediateR(typeof(Program).Assembly);

builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

builder.Services
    .AddOptions<DiscountGrpcConfiguration>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();
var discountGrpcConfiguration = builder.Configuration.TryGetValidatedOptions<DiscountGrpcConfiguration>();
var databaseConfigurations = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();

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
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api/v1/basket")
    .WithTags("Basket API")
    .WithOpenApi()
    .RegisterEndpoints();

app.UseProblemDetailsResponseExceptionHandler();

app.Run();

namespace Basket.API
{
    public class Program
    {
    }
}
