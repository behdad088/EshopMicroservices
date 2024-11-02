namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public record GetOrdersByNameQuery(string Name) : IQuery<GetOrdersByNameResult>;

public record GetOrdersByNameResult(IReadOnlyCollection<OrderDto> Orders);

public class GetOrdersByNameQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.OrderName.Equals(OrderName.From(query.Name)))
            .OrderBy(x => x.OrderName)
            .ToListAsync(cancellationToken);

        var result = MapResult(orders);
        return new GetOrdersByNameResult(result);
    }

    private static OrderDto[] MapResult(IReadOnlyCollection<Domain.Models.Order> orders)
    {
        var result = orders.Select(x => new OrderDto(
            x.Id.Value,
            x.CustomerId!.Value,
            x.OrderName.Value,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.Payment),
            x.Status.Value,
            MapOrderItems(x.OrderItems))).ToArray();

        return result;
    }

    private static AddressDto MapAddress(Address address)
    {
        return new AddressDto(address.FirstName, address.LastName, address.EmailAddress, address.AddressLine,
            address.Country,
            address.State, address.ZipCode);
    }

    private static PaymentDto MapPayment(Payment payment)
    {
        return new PaymentDto(payment.CardName, payment.CardNumber, payment.Expiration, payment.CVV,
            payment.PaymentMethod);
    }

    private static List<OrderItemsDto> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new OrderItemsDto(x.Id.Value, x.OrderId.Value, x.ProductId.Value, x.Quantity, x.Price.Value)).ToList();
    }
}