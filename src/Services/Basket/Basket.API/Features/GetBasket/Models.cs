using System.Text.Json.Serialization;

namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("Items")]
    List<ShoppingCartItem> Items,
    [property: JsonPropertyName("total_price")]
    decimal TotalPrice
    );
    
public record ShoppingCartItem(
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