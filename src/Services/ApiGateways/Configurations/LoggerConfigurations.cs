using System.ComponentModel.DataAnnotations;

namespace ApiGateways.Configurations;


internal sealed record LoggerConfigurations
{
    [ConfigurationKeyName("Logger:elasticsearch")]
    [Required]
    public required string ElasticSearch { get; init; }
}