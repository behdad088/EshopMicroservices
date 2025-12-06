using System.ComponentModel.DataAnnotations;

namespace Order.Query.API.Configurations;

internal sealed record LoggerConfigurations
{
    [ConfigurationKeyName("Logger:elasticsearch")]
    [Required]
    public required string ElasticSearch { get; init; }
}