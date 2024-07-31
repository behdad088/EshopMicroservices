using System.Text.Json.Serialization;

namespace Basket.API.Features.StoreBasket;

public record StoreBasketResponse(bool IsSuccess);

public record StoreBasketRequest(
    [property: JsonPropertyName("shopping_cart")]
    BasketDto ShoppingCart);