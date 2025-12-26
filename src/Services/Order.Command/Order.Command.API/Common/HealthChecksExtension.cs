using eshop.Shared.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Order.Command.API.Common.PostgresHealthCheck;

namespace Order.Command.API.Common;

public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services,
        string configurationString)
    {
        services.AddDefaultHealthChecks()
            .AddSqlServer(new PostgresHealthCheckOptions()
                {
                    ConnectionString = configurationString
                },
                tags: ["ready", "liveness"]
            );

        return services;
    }

    private static IHealthChecksBuilder AddSqlServer(
        this IHealthChecksBuilder builder,
        PostgresHealthCheckOptions options,
        string name = "sqlserver",
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new PostgresServerHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}