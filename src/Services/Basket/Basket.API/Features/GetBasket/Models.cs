namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(
    string Username,
    List<CartItem> Items,
    decimal TotalPrice
) : BasketDto(Username, Items, TotalPrice);