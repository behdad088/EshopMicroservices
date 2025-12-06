using eshop.Shared.CQRS.Query;
using eshop.Shared.Pagination;

namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public record GetOrderByCustomerQuery(
    string CustomerId,
    int PageSize = 10,
    int PageIndex = 0) : IQuery<GetOrderByCustomerResult>;

public record GetOrderByCustomerResult(PaginatedItems<GetOrderByCustomerResponse> Orders);

public class GetOrderByCustomerQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderByCustomerQuery, GetOrderByCustomerResult>
{
    private readonly ILogger _logger = Log.ForContext<GetOrderByCustomerQueryHandler>();
    
    public async Task<GetOrderByCustomerResult> Handle(GetOrderByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        _logger.Information("Get order by Customer Id.");
        var customerId = Guid.Parse(query.CustomerId);
        var pageIndex = query.PageIndex;
        var pageSize = query.PageSize;
        var totalCount = await dbContext.Orders.Where(
                x => x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null)
            .LongCountAsync(cancellationToken);


        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = MapResult(orders, pageIndex, pageSize, totalCount);
        _logger.Information("Successfully retrieved orders by Customer Id.");
        return new GetOrderByCustomerResult(result);
    }

    private static PaginatedItems<GetOrderByCustomerResponse> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders,
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var result = orders.Select(x => new GetOrderByCustomerResponse(
            x.Id.Value,
            x.CustomerId.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<GetOrderByCustomerResponse>(pageIndex, pageSize, totalCount, result);
    }

    private static GetOrderByCustomerResponse.AddressResponse MapAddress(Address address)
    {
        return new GetOrderByCustomerResponse.AddressResponse(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static GetOrderByCustomerResponse.PaymentResponse MapPayment(Payment payment)
    {
        return new GetOrderByCustomerResponse.PaymentResponse(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }

    private static List<GetOrderByCustomerResponse.OrderItemResponse> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new GetOrderByCustomerResponse.OrderItemResponse(
                x.ProductId.Value.ToString(),
                x.Quantity,
                x.Price.Value)).ToList();
    }
}