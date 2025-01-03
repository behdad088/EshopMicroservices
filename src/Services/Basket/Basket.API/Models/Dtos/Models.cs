using System.Text.Json.Serialization;

namespace Basket.API.Models.Dtos;

public record BasketDtoRequest(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("items")]
    List<BasketItem>? Items
);

public record BasketDtoResponse(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("items")]
    IReadOnlyList<BasketItem> Items,
    [property: JsonPropertyName("total_price")]
    decimal TotalPrice
);
    
public record BasketItem(
    [property: JsonPropertyName("quantity")]
    int Quantity,
    [property: JsonPropertyName("color")]
    string Color,
    [property: JsonPropertyName("price")]
    decimal Price,
    [property: JsonPropertyName("product_id")]
    string ProductId,
    [property: JsonPropertyName("product_name")]
    string ProductName);