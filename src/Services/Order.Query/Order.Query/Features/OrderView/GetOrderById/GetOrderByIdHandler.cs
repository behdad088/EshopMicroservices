using FastEndpoints;
using Marten;

namespace Order.Query.Features.OrderView.GetOrderById;

public class GetOrderByIdQuery : ICommand<Result>
{
    public required string OrderId { get; init; }
    public required string CustomerId { get; init; }
}

public record Result
{
    public record NotFound : Result;

    public record Success(GetOrderByIdResult Result) : Result;
}

public record GetOrderByIdResult(
    string Id,
    string CustomerId,
    string OrderName,
    GetOrderByIdResult.Address ShippingAddress,
    GetOrderByIdResult.Address BillingAddress,
    GetOrderByIdResult.Payment PaymentDetails,
    string Status,
    int Version,
    IReadOnlyCollection<GetOrderByIdResult.OrderItem> OrderItems)
{
    public record OrderItem(
        string? ProductId,
        int? Quantity,
        decimal? Price
    );

    public record Address(
        string Firstname,
        string Lastname,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode
    );

    public record Payment(
        string CardName,
        string CardNumber,
        string Expiration,
        string Cvv,
        int PaymentMethod);
}

public class GetOrderByIdHandler(IDocumentSession session) : ICommandHandler<GetOrderByIdQuery, Result>
{
    public async Task<Result> ExecuteAsync(GetOrderByIdQuery command, CancellationToken ct)
    {
        var result = await session
            .Query<OrderView>()
            .FirstOrDefaultAsync(x => x.Id == command.OrderId && x.CustomerId == command.CustomerId,
                token: ct)
            .ConfigureAwait(false);

        if (result is null)
            return new Result.NotFound();

        return new Result.Success(MapResult(result));
    }

    private static GetOrderByIdResult MapResult(OrderView order)
    {
        return new GetOrderByIdResult(
            Id: order.Id,
            CustomerId: order.CustomerId,
            OrderName: order.OrderName,
            ShippingAddress: MapAddress(order.ShippingAddress),
            BillingAddress: MapAddress(order.BillingAddress),
            PaymentDetails: MapPayment(order.PaymentMethod),
            Status: order.OrderStatus,
            Version:  order.Version,
            OrderItems: order.OrderItems.Select(x => 
                new  GetOrderByIdResult.OrderItem(
                    x.ProductId, 
                    x.Quantity,
                    x.Price)).ToArray().AsReadOnly()
            );
    }

    private static GetOrderByIdResult.Address MapAddress(OrderView.Address address)
    {
        return new GetOrderByIdResult.Address(
            Firstname: address.Firstname,
            Lastname: address.Lastname,
            EmailAddress: address.EmailAddress,
            AddressLine: address.AddressLine,
            Country: address.Country,
            State: address.State,
            ZipCode: address.ZipCode);
    }

    private static GetOrderByIdResult.Payment MapPayment(OrderView.Payment payment)
    {
        return new GetOrderByIdResult.Payment(
            CardName: payment.CardName,
            CardNumber: payment.CardNumber,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv,
            PaymentMethod: payment.PaymentMethod);
    }
}