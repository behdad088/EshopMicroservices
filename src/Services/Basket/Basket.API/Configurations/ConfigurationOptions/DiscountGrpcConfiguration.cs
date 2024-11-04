using System.ComponentModel.DataAnnotations;

namespace Basket.API.Configurations.ConfigurationOptions;

public sealed record DiscountGrpcConfiguration
{
    [ConfigurationKeyName("Grpc:Discount")]
    [Required]
    public required string DiscountGrpc { get; init; }
}