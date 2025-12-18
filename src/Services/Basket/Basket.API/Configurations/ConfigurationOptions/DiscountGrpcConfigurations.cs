using System.ComponentModel.DataAnnotations;

namespace Basket.API.Configurations.ConfigurationOptions;

public sealed record DiscountGrpcConfigurations
{
    [ConfigurationKeyName("Grpc:Discount")]
    [Required]
    public required string DiscountGrpc { get; init; }
}