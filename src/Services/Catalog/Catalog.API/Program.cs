using BuildingBlocks.CQRS.Extensions;
using BuildingBlocks.Exceptions.Handler;
using Catalog.API.Common;


var builder = WebApplication.CreateBuilder(args);
builder.Services.RegisterMediateR(typeof(Program).Assembly);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

var app = builder.Build();

app.MapGroup("/api/v1/catalog")
    .WithTags("Catalog API")
    .RegisterEndpoints();
app.UseProblemDetailsResponseExceptionHandler();

app.Run();