using BuildingBlocks.CQRS.Extensions;
using BuildingBlocks.Exceptions.Handler;
using Catalog.API.Common;
using Catalog.API.Data;
using eshop.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.AddDefaultOpenApi();

builder.Services.RegisterMediateR(typeof(Program).Assembly);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();
builder.Services.AddHealthChecks(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialDataMigration>();
}

var app = builder.Build();
app.UseDefaultOpenApi();
app.MapDefaultHealthChecks();

app.MapGroup("/api/v1/catalog")
    .WithTags("Catalog API")
    .RegisterEndpoints();
app.UseProblemDetailsResponseExceptionHandler();

app.Run();