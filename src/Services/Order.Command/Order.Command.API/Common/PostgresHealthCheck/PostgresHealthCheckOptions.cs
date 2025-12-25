using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Order.Command.API.Common.PostgresHealthCheck;

public class PostgresHealthCheckOptions
{
    /// <summary>
    /// The Sql Server connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = "SELECT 1;";

    public NpgsqlDataSource? DataSource { get; internal set; }

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<NpgsqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}