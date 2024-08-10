using System.ComponentModel.DataAnnotations;

namespace Discount.Grpc.Configurations.ConfigurationOptions;

public sealed record DatabaseConfigurations
{
    [ConfigurationKeyName("ConnectionStrings:Database"), Required]
    public required string SqliteDb { get; init; }
};