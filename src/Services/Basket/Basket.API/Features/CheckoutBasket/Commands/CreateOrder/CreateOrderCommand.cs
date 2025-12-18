using Basket.API.ApiClient;
using Basket.API.ApiClient.OrderCommand;
using Refit;

namespace Basket.API.Features.CheckoutBasket.Commands.CreateOrder;

public record CreteOrderCommandParameters
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public string? OrderName { get; set; }
    public ModuleAddress? ShippingAddress { get; set; }
    public ModuleAddress? BillingAddress { get; set; }
    public ModulePayment? Payment { get; set; }
    public List<ModuleOrderItem>? OrderItems { get; set; }

    public record ModuleAddress(
        string? Firstname,
        string? Lastname,
        string? EmailAddress,
        string? AddressLine,
        string? Country,
        string? State,
        string? ZipCode);

    public record ModulePayment(
        string? CardName,
        string? CardNumber,
        string? Expiration,
        string? Cvv,
        int? PaymentMethod);

    public record ModuleOrderItem(
        string ProductId,
        int? Quantity,
        decimal? Price);
};

public record CreateOrderCommandResult
{
    public record Success(string OrderId) : CreateOrderCommandResult;

    public record Failure(FailureReasons Failures) : CreateOrderCommandResult;
}

public class CreateOrderCommand(
    IOrderCommandClient client)
{
    private static readonly ILogger Logger = Log.ForContext<CreateOrderCommand>();

    public async Task<CreateOrderCommandResult> CreateOrderAsync(
        CreteOrderCommandParameters parameters,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Calling order service to creating order");
        var body = MapBody(parameters);
        try
        {
            var response = await client.CreateOrder(
                parameters.CustomerId,
                parameters.OrderId,
                body,
                cancellationToken);
            
            ValidateOrderResponse(response);
            return new CreateOrderCommandResult.Success(response.Id!);
        }
        catch (ApiException e)
        {
            Logger.Error(e, "Received failed response from order service when creating order");
            
            switch ((int)e.StatusCode)
            {
                case StatusCodes.Status400BadRequest:
                {
                    var errorContent = await e.GetContentAsAsync<ValidationErrors>();
                    var errors = errorContent?.Errors?
                        .Select(x => new Error(x.PropertyName, x.Reason))
                        .ToArray() ?? [];
                
                    return new CreateOrderCommandResult.Failure(ClientFailures.OrderCommandClientFailures.CreateOrder
                        .OrderBadRequest(errors));
                }
                case StatusCodes.Status404NotFound:
                    return new CreateOrderCommandResult.Failure(ClientFailures.OrderCommandClientFailures.CreateOrder
                        .OrderNotFound());
                case StatusCodes.Status401Unauthorized:
                    return new CreateOrderCommandResult.Failure(ClientFailures.OrderCommandClientFailures.CreateOrder
                        .Unauthorised);
                case StatusCodes.Status403Forbidden:
                    return new CreateOrderCommandResult.Failure(ClientFailures.OrderCommandClientFailures.CreateOrder
                        .Forbidden);
                default:
                    throw;
            }
        }
    }

    private static void ValidateOrderResponse(CreateOrderResponse response)
    {
        if (response.Id == null)
            throw new Exception("Order id is required");
    }

    private static CreateOrderRequest MapBody(CreteOrderCommandParameters parameters)
    {
        return new CreateOrderRequest
        {
            OrderName = parameters.OrderName,
            ShippingAddress = MapAddress(parameters.ShippingAddress),
            BillingAddress = MapAddress(parameters.BillingAddress),
            Payment = MapPayment(parameters.Payment),
            OrderItems = parameters.OrderItems?.Select(x =>
                new CreateOrderRequest.ModuleOrderItem(x.ProductId, x.Quantity, x.Price)).ToList()
        };
    }

    private static CreateOrderRequest.ModulePayment? MapPayment(
        CreteOrderCommandParameters.ModulePayment? payment)
    {
        if (payment == null)
            return null;
        
        return new CreateOrderRequest.ModulePayment(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.Cvv,
            payment.PaymentMethod);
    }

    private static CreateOrderRequest.ModuleAddress? MapAddress(
        CreteOrderCommandParameters.ModuleAddress? address)
    {
        if  (address == null)
            return null;

        return new CreateOrderRequest.ModuleAddress(
            Firstname: address.Firstname,
            Lastname: address.Lastname,
            EmailAddress: address.EmailAddress,
            AddressLine: address.AddressLine,
            Country: address.Country,
            State: address.State,
            ZipCode: address.ZipCode);
    }
}