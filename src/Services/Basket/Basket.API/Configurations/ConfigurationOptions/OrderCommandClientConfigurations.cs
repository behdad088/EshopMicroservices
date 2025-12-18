using System.ComponentModel.DataAnnotations;

namespace Basket.API.Configurations.ConfigurationOptions;

internal sealed record OrderCommandClientConfigurations
{
    [ConfigurationKeyName("OrderCommandClient:BaseUrl")]
    [Required]
    public required string BaseUrl { get; init; }
}