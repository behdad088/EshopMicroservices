using BuildingBlocks.CQRS.Command;
using Discount;

namespace Basket.API.Features.StoreBasket;

public record StoreBasketCommand(ShoppingCart? ShoppingCart) : ICommand<StoreBasketResult>;
public record StoreBasketResult(ShoppingCart ShoppingCart);

public class StoreBasketCommandHandler(
    IBasketRepository basketRepository,
    DiscountProtoService.DiscountProtoServiceClient discountClient) : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand request, CancellationToken cancellationToken)
    {
        await ApplyDiscountAsync(request.ShoppingCart?.Items!, cancellationToken).ConfigureAwait(false);

        var result = await basketRepository.StoreBasketAsync(request.ShoppingCart!, cancellationToken)
            .ConfigureAwait(false);
        
        return new StoreBasketResult(result);
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
}