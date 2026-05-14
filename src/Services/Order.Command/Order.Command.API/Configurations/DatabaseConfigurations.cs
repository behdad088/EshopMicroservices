using System.ComponentModel.DataAnnotations;

namespace Order.Command.API.Configurations;

internal sealed record DatabaseConfigurations
{
    [ConfigurationKeyName("ConnectionStrings:Database")]
    [Required]
    public required string PostgresDatabase { get; init; }
}
