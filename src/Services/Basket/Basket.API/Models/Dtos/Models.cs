using System.Text.Json.Serialization;

namespace Basket.API.Models.Dtos;

public record BasketDto(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("Items")]
    List<CartItem> Items,
    [property: JsonPropertyName("total_price")]
    decimal TotalPrice
);
    
public record CartItem(
    [property: JsonPropertyName("quantity")]
    int Quantity,
    [property: JsonPropertyName("color")]
    string Color,
    [property: JsonPropertyName("price")]
    decimal Price,
    [property: JsonPropertyName("Produce_id")]
    Guid ProduceId,
    [property: JsonPropertyName("product_name")]
    string ProductName);