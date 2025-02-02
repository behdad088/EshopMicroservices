using BuildingBlocks.Pagination;

namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public record GetOrdersByNameQuery(
    string Name,
    int PageSize = 10,
    int PageIndex = 0) : IQuery<GetOrdersByNameResult>;

public record GetOrdersByNameResult(PaginatedItems<GetOrderByNameParameter> Orders);

public class GetOrdersByNameQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PageIndex;
        var pageSize = query.PageSize;
        var totalCount = await dbContext.Orders.Where(
                x => x.CustomerId.Equals(OrderName.From(query.Name)) && x.DeleteDate == null)
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
        return new GetOrdersByNameResult(result);
    }

    private static PaginatedItems<GetOrderByNameParameter> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders,
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var result = orders.Select(x => new GetOrderByNameParameter(
            x.Id.Value,
            x.CustomerId.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<GetOrderByNameParameter>(pageIndex, pageSize, totalCount, result);
    }

    private static AddressParameter MapAddress(Address address)
    {
        return new AddressParameter(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static PaymentParameter MapPayment(Payment payment)
    {
        return new PaymentParameter(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }

    private static List<OrderItemParameter> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new OrderItemParameter(
                x.Id.Value.ToString(),
                x.ProductId.Value.ToString(),
                x.Quantity,
                x.Price.Value)).ToList();
    }
}