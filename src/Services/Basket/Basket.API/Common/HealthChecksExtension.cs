using Basket.API.Configurations;
using Basket.API.Configurations.ConfigurationOptions;
using BuildingBlocks.HealthChecks;
using eshop.Shared;

namespace Basket.API.Common;

public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDefaultHealthChecks(configuration);
        var databaseConfiguration = configuration.TryGetValidatedOptions<DatabaseConfigurations>();
        
        services.AddHealthChecks()
            .AddNpgSql(databaseConfiguration.PostgresDb, name: "postgres", tags: ["ready", "liveness"])
            .AddRedis(databaseConfiguration.Redis, "redis", tags: ["ready", "liveness"]);

        return services;
    }
}