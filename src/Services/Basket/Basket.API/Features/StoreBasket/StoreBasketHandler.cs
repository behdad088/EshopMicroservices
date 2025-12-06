using Discount;
using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;

namespace Basket.API.Features.StoreBasket;

public record StoreBasketCommand(BasketDtoRequest? ShoppingCart) : ICommand<StoreBasketResult>;

public record StoreBasketResult(BasketDtoResponse ShoppingCart);

public class StoreBasketCommandHandler(
    IBasketRepository basketRepository,
    DiscountProtoService.DiscountProtoServiceClient discountClient)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    private readonly ILogger _logger = Log.ForContext<StoreBasketCommandHandler>();
    public async Task<StoreBasketResult> Handle(StoreBasketCommand request, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.Username, request.ShoppingCart!.Username);

        _logger.Information("Store Basket for user");
        
        var shoppingCart = MapToShoppingCart(request.ShoppingCart!);
        await ApplyDiscountAsync(shoppingCart.Items, cancellationToken)
            .ConfigureAwait(false);

        var result = await basketRepository.StoreBasketAsync(shoppingCart, cancellationToken)
            .ConfigureAwait(false);

        return MapToResult(result);
    }

    private static StoreBasketResult MapToResult(ShoppingCart shoppingCart)
    {
        return new StoreBasketResult(
            new BasketDtoResponse(
                shoppingCart.Username,
                Items: shoppingCart.Items.Select(x =>
                    new BasketItem(
                        x.Quantity,
                        x.Color,
                        x.Price,
                        x.ProductId,
                        x.ProductName)).ToList(),
                shoppingCart.TotalPrice));
    }

    private async Task ApplyDiscountAsync(IReadOnlyList<ShoppingCartItem> items, CancellationToken token)
    {
        foreach (var item in items)
        {
            var coupon = await discountClient.GetDiscountAsync(
                    new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: token)
                .ConfigureAwait(false);

            item.Price -= coupon.Amount;
        }
    }

    private static ShoppingCart MapToShoppingCart(BasketDtoRequest shoppingCart)
    {
        return new ShoppingCart(
            username: shoppingCart.Username)
        {
            Items = shoppingCart.Items!.Select(x => new ShoppingCartItem
            {
                ProductId = x.ProductId,
                Color = x.Color,
                Price = x.Price,
                ProductName = x.ProductName,
                Quantity = x.Quantity
            }).ToList()
        };
    }
}