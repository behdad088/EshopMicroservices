namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(
    string Username,
    IReadOnlyList<BasketItem> Items,
    decimal TotalPrice
) : BasketDtoResponse(Username, Items, TotalPrice);