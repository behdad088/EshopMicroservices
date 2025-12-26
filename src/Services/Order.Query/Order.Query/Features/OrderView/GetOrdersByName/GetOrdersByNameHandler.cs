using FastEndpoints;

namespace Order.Query.Features.OrderView.GetOrdersByName;

public class GetOrdersByOrderNameQuery : ICommand<PaginatedItems<GetOrdersByOrderNameResult>>
{
    public required string OrderName { get; init; }
    public required int PageSize { get; init; }
    public required int PageIndex { get; init; }
}

public record GetOrdersByOrderNameResult(
    string Id,
    string CustomerId,
    string OrderName,
    GetOrdersByOrderNameResult.Address ShippingAddress,
    GetOrdersByOrderNameResult.Address BillingAddress,
    GetOrdersByOrderNameResult.Payment PaymentDetails,
    string Status,
    IReadOnlyCollection<GetOrdersByOrderNameResult.OrderItem> OrderItems)
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
    ICommandHandler<GetOrdersByOrderNameQuery, PaginatedItems<GetOrdersByOrderNameResult>>
{
    private readonly ILogger _logger = Log.ForContext<GetOrdersByCustomerHandler>();
    public async Task<PaginatedItems<GetOrdersByOrderNameResult>> ExecuteAsync(
        GetOrdersByOrderNameQuery command,
        CancellationToken ct)
    {
        _logger.Information("Get orders by name");
        var pageSize = command.PageSize;
        var pageIndex = command.PageIndex;
        
        var totalCount = await session.Query<OrderView>()
            .Where(x => x.OrderName == command.OrderName && x.DeletedDate == null)
            .LongCountAsync(ct).ConfigureAwait(false);

        var dbResult = await session.Query<OrderView>()
            .Where(x => x.OrderName == command.OrderName && x.DeletedDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = dbResult.Select(MapResult).ToArray();
        _logger.Information("Successfully retrieved orders by name");
        return new  PaginatedItems<GetOrdersByOrderNameResult>(pageIndex, pageSize, totalCount, result);
    }
    
    private static GetOrdersByOrderNameResult MapResult(OrderView order)
    {
        return new GetOrdersByOrderNameResult(
            Id: order.Id,
            CustomerId: order.CustomerId!,
            OrderName: order.OrderName!,
            ShippingAddress: MapAddress(order.ShippingAddress),
            BillingAddress: MapAddress(order.BillingAddress),
            PaymentDetails: MapPayment(order.PaymentMethod),
            Status: order.OrderStatus!,
            OrderItems: order.OrderItems.Select(x => 
                new  GetOrdersByOrderNameResult.OrderItem(
                    x.ProductId, 
                    x.Quantity,
                    x.Price)).ToArray().AsReadOnly()
        );
    }

    private static GetOrdersByOrderNameResult.Address MapAddress(OrderView.Address address)
    {
        return new GetOrdersByOrderNameResult.Address(
            Firstname: address.Firstname!,
            Lastname: address.Lastname!,
            EmailAddress: address.EmailAddress!,
            AddressLine: address.AddressLine!,
            Country: address.Country!,
            State: address.State!,
            ZipCode: address.ZipCode!);
    }

    private static GetOrdersByOrderNameResult.Payment MapPayment(OrderView.Payment payment)
    {
        return new GetOrdersByOrderNameResult.Payment(
            CardName: payment.CardName!,
            CardNumber: payment.CardNumber!,
            Expiration: payment.Expiration!,
            Cvv: payment.Cvv!,
            PaymentMethod: payment.PaymentMethod);
    }
}