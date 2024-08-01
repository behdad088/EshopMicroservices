namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(
    string Username,
    List<BasketItem> Items,
    decimal TotalPrice
) : BasketDtoResponse(Username, Items, TotalPrice);