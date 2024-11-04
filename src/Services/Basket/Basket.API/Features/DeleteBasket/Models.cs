namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketResponse(
    [property: JsonPropertyName("is_success")]bool IsSuccess);