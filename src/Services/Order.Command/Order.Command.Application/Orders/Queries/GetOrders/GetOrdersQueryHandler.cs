using BuildingBlocks.Pagination;
using eshop.Shared.CQRS.Query;

namespace Order.Command.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    int PageSize = 10,
    int PageIndex = 0) : IQuery<GetOrdersResult>;

public record GetOrdersResult(PaginatedItems<GetOrderResponse> Orders);

public class GetOrdersQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PageIndex;
        var pageSize = query.PageSize;
        var totalCount = await dbContext.Orders
            .Where(x => x.DeleteDate == null)
            .LongCountAsync(cancellationToken).ConfigureAwait(false);

        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .Where(x => x.DeleteDate == null)
            .OrderBy(x => x.OrderName)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var orderItems = MapResult(orders, pageIndex, pageSize, totalCount);

        return new GetOrdersResult(orderItems);
    }

    private static PaginatedItems<GetOrderResponse> MapResult(
        IReadOnlyCollection<Domain.Models.Order> orders,
        int pageIndex,
        int pageSize,
        long totalCount)
    {
        var ordersDto = orders.Select(x => new GetOrderResponse(
            x.Id.Value,
            x.CustomerId.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return new PaginatedItems<GetOrderResponse>(pageIndex, pageSize, totalCount, ordersDto);
    }

    private static GetOrderResponse.AddressResponse MapAddress(Address address)
    {
        return new GetOrderResponse.AddressResponse(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static GetOrderResponse.PaymentResponse MapPayment(Payment payment)
    {
        return new GetOrderResponse.PaymentResponse(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }

    private static List<GetOrderResponse.OrderItemResponse> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new GetOrderResponse.OrderItemResponse(
                x.ProductId.Value.ToString(),
                x.Quantity,
                x.Price.Value)).ToList();
    }
}