namespace Basket.API.Features.StoreBasket;

public record StoreBasketResponse(bool IsSuccess);

public record StoreBasketRequest(
    string Username,
    List<CartItem> Items,
    decimal TotalPrice
) : BasketDto(Username, Items, TotalPrice);