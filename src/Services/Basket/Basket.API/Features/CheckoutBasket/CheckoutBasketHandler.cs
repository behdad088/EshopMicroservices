using Basket.API.ApiClient;
using Basket.API.Features.CheckoutBasket.Commands.CreateOrder;
using Discount;
using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;

namespace Basket.API.Features.CheckoutBasket;

public record CheckoutBasketCommand(CheckoutRequestModel? CheckoutRequestModel) : ICommand<Result>;

public abstract record Result
{
    public record Success(string OrderId) : Result;
    public record BasketNotFound : Result;
    public record ApiClientError(FailureReasons FailureReason) : Result;
}

public class CheckoutBasketCommandHandler(
    IBasketRepository basketRepository,
    CreateOrderCommand createOrderCommand,
    DiscountProtoService.DiscountProtoServiceClient discountClient)
    : ICommandHandler<CheckoutBasketCommand, Result>
{
    private readonly ILogger _logger = Log.ForContext<CheckoutBasketCommandHandler>();

    public async Task<Result> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.Username, request.CheckoutRequestModel?.Username);

        _logger.Information("Checkout Basket for user");

        var basket = await basketRepository.GetBasketAsync(request.CheckoutRequestModel!.Username!, cancellationToken)
            .ConfigureAwait(false);

        if (basket is null)
            return new Result.BasketNotFound();

        // Persist the order ID before calling the Order Service so that a failed basket
        // delete on the first attempt can be safely retried with the same idempotent ID.
        if (basket.PendingCheckoutOrderId is null)
        {
            basket.PendingCheckoutOrderId = Ulid.NewUlid().ToString();
            await basketRepository.StoreBasketAsync(basket, cancellationToken).ConfigureAwait(false);
        }

        // Fetch discounts at checkout time so prices are always current.
        var effectivePrices = await GetEffectivePricesAsync(basket.Items, cancellationToken).ConfigureAwait(false);

        var response = await createOrderCommand.CreateOrderAsync(
            MapParameter(request.CheckoutRequestModel, basket, effectivePrices), cancellationToken).ConfigureAwait(false);

        switch (response)
        {
            case CreateOrderCommandResult.Success success:
                _logger.Information("Basket was successfully checked out with {Id}", success.OrderId);
                await basketRepository.DeleteBasketAsync(request.CheckoutRequestModel.Username!, cancellationToken)
                    .ConfigureAwait(false);
                return new Result.Success(success.OrderId);
            case CreateOrderCommandResult.Failure failure:
                return new Result.ApiClientError(failure.Failures);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<IReadOnlyList<decimal>> GetEffectivePricesAsync(
        IReadOnlyList<ShoppingCartItem> items, CancellationToken token)
    {
        var prices = new List<decimal>(items.Count);
        foreach (var item in items)
        {
            var coupon = await discountClient.GetDiscountAsync(
                    new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: token)
                .ConfigureAwait(false);
            prices.Add(item.Price - coupon.Amount);
        }
        return prices;
    }

    private static CreteOrderCommandParameters MapParameter(
        CheckoutRequestModel checkoutRequestModel,
        ShoppingCart shoppingCart,
        IReadOnlyList<decimal> effectivePrices)
    {
        return new CreteOrderCommandParameters
        {
            OrderId = shoppingCart.PendingCheckoutOrderId,
            CustomerId = checkoutRequestModel.CustomerId!,
            OrderName = checkoutRequestModel.OrderName,
            ShippingAddress = MapAddress(checkoutRequestModel.BillingAddress!),
            BillingAddress = MapAddress(checkoutRequestModel.BillingAddress!),
            Payment = MapPayment(checkoutRequestModel.Payment!),
            OrderItems = shoppingCart.Items
                .Zip(effectivePrices, (item, price) => new CreteOrderCommandParameters.ModuleOrderItem(
                    item.ProductId!, item.Quantity, price))
                .ToList()
        };
    }

    private static CreteOrderCommandParameters.ModulePayment MapPayment(
        CheckoutRequestModel.ModulePayment payment)
    {
        return new CreteOrderCommandParameters.ModulePayment(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.Cvv,
            payment.PaymentMethod);
    }

    private static CreteOrderCommandParameters.ModuleAddress MapAddress(
        CheckoutRequestModel.ModuleAddress address)
    {
        return new CreteOrderCommandParameters.ModuleAddress(
            address.Firstname,
            address.Lastname,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }
}