using BuildingBlocks.Pagination;

namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public record GetOrderByCustomerQuery(
    string CustomerId,
    int PageSize = 10,
    int PageIndex = 0) : IQuery<GetOrderByCustomerResult>;

public record GetOrderByCustomerResult(PaginatedItems<GetOrderByCustomerParameter> Orders);

public class GetOrderByCustomerQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderByCustomerQuery, GetOrderByCustomerResult>
{
    public async Task<GetOrderByCustomerResult> Handle(GetOrderByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = Guid.Parse(query.CustomerId);
        var pageIndex = query.PageIndex;
        var pageSize = query.PageSize;
        var totalCount = await dbContext.Orders.Where(
            x => x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null).LongCountAsync(cancellationToken);
        
        
        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = MapResult(orders, pageIndex, pageSize, totalCount);
        return new GetOrderByCustomerResult(result);
    }

    private static PaginatedItems<GetOrderByCustomerParameter> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders,
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var result = orders.Select(x => new GetOrderByCustomerParameter(
            x.Id.Value,
            x.CustomerId.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<GetOrderByCustomerParameter>(pageIndex, pageSize, totalCount, result);
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