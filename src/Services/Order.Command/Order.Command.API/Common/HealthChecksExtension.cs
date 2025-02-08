using BuildingBlocks.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Order.Command.API.Common.SqlHealthCheck;

namespace Order.Command.API.Common;

public static class HealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services,
        string configurationString)
    {
        services.AddDefaultHealthChecks()
            .AddSqlServer(new SqlServerHealthCheckOptions()
                {
                    ConnectionString = configurationString
                },
                tags: ["ready", "liveness"]
            );

        return services;
    }

    private static IHealthChecksBuilder AddSqlServer(
        this IHealthChecksBuilder builder,
        SqlServerHealthCheckOptions options,
        string name = "sqlserver",
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name,
            _ => new SqlServerHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}