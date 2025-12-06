using System.ComponentModel.DataAnnotations;

namespace Order.Command.API.Configurations;

internal sealed record LoggerConfigurations
{
    [ConfigurationKeyName("Logger:elasticsearch")]
    [Required]
    public required string ElasticSearch { get; init; }
}