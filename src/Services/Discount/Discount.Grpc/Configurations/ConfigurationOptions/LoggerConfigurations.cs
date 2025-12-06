using System.ComponentModel.DataAnnotations;

namespace Discount.Grpc.Configurations.ConfigurationOptions;

internal sealed record LoggerConfigurations
{
    [ConfigurationKeyName("Logger:elasticsearch")]
    [Required]
    public required string ElasticSearch { get; init; }
}