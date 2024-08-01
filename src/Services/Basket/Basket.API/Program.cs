using Basket.API.Common;
using BuildingBlocks.CQRS.Extensions;
using BuildingBlocks.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);
builder.Services.RegisterMediateR(typeof(Program).Assembly);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
    options.Schema.For<ShoppingCart>()
        .UseNumericRevisions(true);

}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/api/v1/basket")
    .WithTags("Basket API")
    .WithOpenApi()
    .RegisterEndpoints();

app.UseProblemDetailsResponseExceptionHandler();


app.Run();