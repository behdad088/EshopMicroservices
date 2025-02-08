using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Order.Command.API.Common.SqlHealthCheck;

public class SqlServerHealthCheckOptions
{
    /// <summary>
    /// The Sql Server connection string to be used.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The query to be executed.
    /// </summary>
    public string CommandText { get; set; } = "SELECT 1;";

    /// <summary>
    /// An optional action executed before the connection is opened in the health check.
    /// </summary>
    public Action<SqlConnection>? Configure { get; set; }

    /// <summary>
    /// An optional delegate to build health check result.
    /// </summary>
    public Func<object?, HealthCheckResult>? HealthCheckResultBuilder { get; set; }
}