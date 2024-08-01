namespace Basket.API.Features.StoreBasket;

public record StoreBasketResponse(
    [property: JsonPropertyName("shopping_cart")]
    BasketDtoResponse ShoppingCart);

public record StoreBasketRequest(
    [property: JsonPropertyName("shopping_cart")]
    BasketDtoRequest ShoppingCart);