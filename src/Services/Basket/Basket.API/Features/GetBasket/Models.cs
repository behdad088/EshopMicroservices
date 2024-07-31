namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(
    string Username,
    List<BasketItem> Items,
    decimal TotalPrice
) : BasketDto(Username, Items, TotalPrice);