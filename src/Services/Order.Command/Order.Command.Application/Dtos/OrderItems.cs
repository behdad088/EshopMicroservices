using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record OrderItems(
    [property: JsonPropertyName("id")] 
    string Id,
    [property: JsonPropertyName("order_id")]
    string? OrderId,
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);