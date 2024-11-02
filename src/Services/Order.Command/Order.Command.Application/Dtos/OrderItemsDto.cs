using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record OrderItemsDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("order_id")]
    Guid? OrderId,
    [property: JsonPropertyName("product_id")]
    Guid? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);