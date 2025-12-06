using eshop.Shared.CQRS.Query;
using eshop.Shared.Pagination;

namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public record GetOrdersByNameQuery(
    string Name,
    int PageSize = 10,
    int PageIndex = 0) : IQuery<GetOrdersByNameResult>;

public record GetOrdersByNameResult(PaginatedItems<GetOrderByNameResponse> Orders);

public class GetOrdersByNameQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    private readonly ILogger _logger = Log.ForContext<GetOrdersByNameQueryHandler>();
    public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
    {
        _logger.Information("Get orders by name");
        var pageIndex = query.PageIndex;
        var pageSize = query.PageSize;
        var totalCount = await dbContext.Orders.Where(
                x => x.OrderName.Equals(OrderName.From(query.Name)) && x.DeleteDate == null)
            .AsNoTracking()
            .LongCountAsync(cancellationToken);

        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.OrderName.Equals(OrderName.From(query.Name)) && x.DeleteDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = MapResult(orders, pageIndex, pageSize, totalCount);
        _logger.Information("Successfully retrieved orders by name");
        return new GetOrdersByNameResult(result);
    }

    private static PaginatedItems<GetOrderByNameResponse> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders,
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var result = orders.Select(x => new GetOrderByNameResponse(
            x.Id.Value,
            x.CustomerId.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<GetOrderByNameResponse>(pageIndex, pageSize, totalCount, result);
    }

    private static GetOrderByNameResponse.AddressResponse MapAddress(Address address)
    {
        return new GetOrderByNameResponse.AddressResponse(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static GetOrderByNameResponse.PaymentResponse MapPayment(Payment payment)
    {
        return new GetOrderByNameResponse.PaymentResponse(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }

    private static List<GetOrderByNameResponse.OrderItemResponse> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new GetOrderByNameResponse.OrderItemResponse(
                x.ProductId.Value.ToString(),
                x.Quantity,
                x.Price.Value)).ToList();
    }
}