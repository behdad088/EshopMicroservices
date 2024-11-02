using BuildingBlocks.HealthChecks;

namespace Order.Command.API.Common;

public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDefaultHealthChecks();
        // should add db health check here

        return services;
    }
}