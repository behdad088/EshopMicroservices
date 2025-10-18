using BuildingBlocks.HealthChecks;
using eshop.Shared;
using Order.Query.EventProcessor.Configurations;

namespace Order.Query.EventProcessor.Health;

public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDefaultHealthChecks();
        var databaseConfiguration = configuration.TryGetValidatedOptions<DatabaseConfigurations>();

        services.AddHealthChecks()
            .AddNpgSql(databaseConfiguration.PostgresDb, name: "postgres", tags: ["ready", "liveness"]);

        return services;
    }
}