using Discount.Grpc.Configurations.ConfigurationOptions;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using eshop.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var databaseConfigurations = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(options =>
    options.UseSqlite(databaseConfigurations.SqliteDb));

builder.Services.MigrateDatabase();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

namespace Discount.Grpc
{
    public class Program
    {
    }
}