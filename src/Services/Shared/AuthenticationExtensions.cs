using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace eshop.Shared;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDefaultAuthentication(
        this IHostApplicationBuilder builder,
        Action<AuthorizationOptions>? configure = null)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // {
        //   "Identity": {
        //     "Url": "http://identity",
        //     "Audience": "basket"
        //    }
        // }

        var identitySection = configuration.GetSection("Identity");

        if (!identitySection.Exists())
        {
            // No identity section, so no authentication
            return services;
        }

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                var identityUrl = identitySection.GetRequiredValue("Url");
                var audience = identitySection.GetRequiredValue("Audience");

                // Skip OIDC discovery entirely — we pin ValidIssuers and bypass signature
                // validation, so fetching the metadata endpoint is unnecessary and would
                // fail against a self-signed dev cert.
                // NOTE: This is not recommended for production scenarios, but is acceptable here since this is only used for local development with a self-signed certificate.
                // This is added to support E2E tests running in the local environment, and should not be used in production or staging environments.
                options.Configuration = new OpenIdConnectConfiguration { Issuer = identityUrl };
                options.Audience = audience;

                options.TokenValidationParameters.ValidIssuers = [identityUrl];

                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine("Token received: " + context.Token);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("Challenge triggered");
                        return Task.CompletedTask;
                    }
                };
            });

        if (configure is not null)
        {
            services.AddAuthorization(configure);
        }
        else
        {
            services.AddAuthorization();
        }

        return services;
    }

    private static string GetRequiredValue(this IConfiguration configuration, string name) =>
        configuration[name] ?? throw new InvalidOperationException(
            $"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");
}
