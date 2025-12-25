using Basket.API.ApiClient;
using Basket.API.Features.CheckoutBasket.Commands.CreateOrder;
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
    CreateOrderCommand  createOrderCommand)
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

        var response = await createOrderCommand.CreateOrderAsync(
            MapParameter(request.CheckoutRequestModel, basket), cancellationToken).ConfigureAwait(false);

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

    private static CreteOrderCommandParameters MapParameter(
        CheckoutRequestModel checkoutRequestModel,
        ShoppingCart shoppingCart)
    {
        return new CreteOrderCommandParameters
        {
            OrderId = Ulid.NewUlid().ToString(),
            CustomerId = checkoutRequestModel.CustomerId!,
            OrderName = checkoutRequestModel.OrderName,
            ShippingAddress = MapAddress(checkoutRequestModel.BillingAddress!),
            BillingAddress = MapAddress(checkoutRequestModel.BillingAddress!),
            Payment = MapPayment(checkoutRequestModel.Payment!),
            OrderItems = shoppingCart.Items.Select(x => new CreteOrderCommandParameters.ModuleOrderItem(
                x.ProductId!, x.Quantity, x.Price)).ToList()
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
        CheckoutRequestModel.ModuleAddress address
    )
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