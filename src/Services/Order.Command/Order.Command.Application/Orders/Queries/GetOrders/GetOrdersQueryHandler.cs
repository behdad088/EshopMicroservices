using BuildingBlocks.Pagination;

namespace Order.Command.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(PaginationRequest PaginationRequest) : IQuery<GetOrdersResult>;
public record GetOrdersResult(PaginatedItems<OrderDto> Orders);

public class GetOrdersQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;
        var totalCount = await dbContext.Orders.LongCountAsync(cancellationToken);
        
        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .OrderBy(x => x.OrderName.Value)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        var orderItems = MapResult(orders, pageIndex, pageSize, totalCount);

        return new GetOrdersResult(orderItems);
    }
    
    private static PaginatedItems<OrderDto> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders, 
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var ordersDto = orders.Select(x => new OrderDto(
            x.Id.Value,
            x.CustomerId!.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<OrderDto>(pageIndex, pageSize, totalCount, ordersDto);
    }

    private static AddressDto MapAddress(Address address) =>
        new(address.FirstName, address.LastName, address.EmailAddress, address.AddressLine, address.Country,
            address.State, address.ZipCode);

    private static PaymentDto MapPayment(Payment payment) =>
        new(payment.CardName, payment.CardNumber, payment.Expiration, payment.CVV, payment.PaymentMethod);

    private static List<OrderItemsDto> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems) =>
        orderItems.Select(x =>
            new OrderItemsDto(x.Id.Value, x.OrderId.Value, x.ProductId.Value, x.Quantity, x.Price.Value)).ToList();
}