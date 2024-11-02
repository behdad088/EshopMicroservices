using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record OrderItemsDto(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("order_id")]
    Ulid? OrderId,
    [property: JsonPropertyName("product_id")]
    Ulid? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);