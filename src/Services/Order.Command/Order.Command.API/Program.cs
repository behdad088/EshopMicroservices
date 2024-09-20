using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultHealthChecks();

var app = builder.Build();
app.MapDefaultHealthChecks();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api/v1/order/command/")
    .WithTags("Order Command API")
    .WithOpenApi();

app.UseProblemDetailsResponseExceptionHandler();
app.Run();