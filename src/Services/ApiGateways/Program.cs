using System.Security.Cryptography.X509Certificates;
using ApiGateways.Configurations;
using eshop.Shared.Configurations;
using eshop.Shared.HealthChecks;
using eshop.Shared.Logger;
using eshop.Shared.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;

const string serviceName = "api.gateway";
builder.Services.AddOpenTelemetryOtl(serviceName);
var configuration = builder.Configuration;
builder.Services.TrySetConfiguration<LoggerConfigurations>(builder.Configuration, out var loggerConfigurations);
builder.SetupLogging("Api Gateway", environment, loggerConfigurations.ElasticSearch);
builder.Services.AddDefaultHealthChecks();

builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration.GetRequiredSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        var caCertPath = configuration["Certificates:CaPath"];
        if (!string.IsNullOrEmpty(caCertPath))
        {
            var caCert = X509CertificateLoader.LoadCertificateFromFile(caCertPath);
            handler.SslOptions.RemoteCertificateValidationCallback = (_, cert, chain, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
                    return true;

                using var customChain = new X509Chain();
                customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                customChain.ChainPolicy.CustomTrustStore.Add(caCert);
                return customChain.Build(new X509Certificate2(cert!));
            };
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

// app.UseHttpsRedirection();
app.MapDefaultHealthChecks();
app.MapReverseProxy();


app.Run();
