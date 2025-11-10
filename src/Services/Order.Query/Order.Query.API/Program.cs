using eshop.Shared;
using eshop.Shared.Exceptions.Handler;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Order.Query.API.Authorization;
using Order.Query.API.Configurations;
using Order.Query.PostgresConfig;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<DatabaseConfigurations>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotationsRecursively()
    .ValidateOnStart();

var connectionString = builder.Configuration.TryGetValidatedOptions<DatabaseConfigurations>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();
builder.Services.AddPostgresDb(connectionString.PostgresDb);

builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
builder.AddDefaultAuthentication(Policies.ConfigureAuthorization);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
    c.Versioning.DefaultVersion = 1;
    c.Serializer.Options.PropertyNamingPolicy = null;
    c.Serializer.Options.AllowOutOfOrderMetadataProperties = true;
});

app.UseProblemDetailsResponseExceptionHandler();

app.Run();


namespace Order.Query.Api
{
    public class Program
    {
    }
}