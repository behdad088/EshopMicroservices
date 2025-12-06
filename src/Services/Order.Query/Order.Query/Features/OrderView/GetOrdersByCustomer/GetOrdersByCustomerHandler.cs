using FastEndpoints;

namespace Order.Query.Features.OrderView.GetOrdersByCustomer;

public class GetOrdersByCustomerQuery : ICommand<PaginatedItems<GetOrdersByCustomerResult>>
{
    public required string CustomerId { get; init; }
    public required int PageSize { get; init; }
    public required int PageIndex { get; init; }
}

public record GetOrdersByCustomerResult(
    string Id,
    string CustomerId,
    string OrderName,
    GetOrdersByCustomerResult.Address ShippingAddress,
    GetOrdersByCustomerResult.Address BillingAddress,
    GetOrdersByCustomerResult.Payment PaymentDetails,
    string Status,
    IReadOnlyCollection<GetOrdersByCustomerResult.OrderItem> OrderItems)
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

public class GetOrdersByCustomerHandler(IDocumentSession session) : 
    ICommandHandler<GetOrdersByCustomerQuery, PaginatedItems<GetOrdersByCustomerResult>>
{
    private readonly ILogger _logger = Log.ForContext<GetOrdersByCustomerHandler>();
    public async Task<PaginatedItems<GetOrdersByCustomerResult>> ExecuteAsync(
        GetOrdersByCustomerQuery command,
        CancellationToken ct)
    {
        _logger.Information("Get order by Customer Id.");
        var pageSize = command.PageSize;
        var pageIndex = command.PageIndex;
        
        var totalCount = await session.Query<OrderView>()
            .Where(x => x.CustomerId == command.CustomerId && x.DeletedDate == null)
            .LongCountAsync(ct).ConfigureAwait(false);

        var dbResult = await session.Query<OrderView>()
            .Where(x => x.CustomerId == command.CustomerId && x.DeletedDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = dbResult.Select(MapResult).ToArray();
        _logger.Information("Successfully retrieved orders by Customer Id.");
        return new  PaginatedItems<GetOrdersByCustomerResult>(pageIndex, pageSize, totalCount, result);
    }
    
    private static GetOrdersByCustomerResult MapResult(OrderView order)
    {
        return new GetOrdersByCustomerResult(
            Id: order.Id,
            CustomerId: order.CustomerId,
            OrderName: order.OrderName,
            ShippingAddress: MapAddress(order.ShippingAddress),
            BillingAddress: MapAddress(order.BillingAddress),
            PaymentDetails: MapPayment(order.PaymentMethod),
            Status: order.OrderStatus,
            OrderItems: order.OrderItems.Select(x => 
                new  GetOrdersByCustomerResult.OrderItem(
                    x.ProductId, 
                    x.Quantity,
                    x.Price)).ToArray().AsReadOnly()
        );
    }

    private static GetOrdersByCustomerResult.Address MapAddress(OrderView.Address address)
    {
        return new GetOrdersByCustomerResult.Address(
            Firstname: address.Firstname,
            Lastname: address.Lastname,
            EmailAddress: address.EmailAddress,
            AddressLine: address.AddressLine,
            Country: address.Country,
            State: address.State,
            ZipCode: address.ZipCode);
    }

    private static GetOrdersByCustomerResult.Payment MapPayment(OrderView.Payment payment)
    {
        return new GetOrdersByCustomerResult.Payment(
            CardName: payment.CardName,
            CardNumber: payment.CardNumber,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv,
            PaymentMethod: payment.PaymentMethod);
    }
}