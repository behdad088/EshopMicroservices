using System.ComponentModel.DataAnnotations;

namespace Order.Query.API.Configurations;

public class DatabaseConfigurations
{
    [ConfigurationKeyName("ConnectionStrings:Database")]
    [Required]
    public required string PostgresDb { get; init; }
}