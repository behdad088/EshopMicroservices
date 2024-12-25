namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public record GetOrderByCustomerQuery(string CustomerId) : IQuery<GetOrderByCustomerResult>;

public record GetOrderByCustomerResult(IEnumerable<OrderDto> Orders);

public class GetOrderByCustomerQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderByCustomerQuery, GetOrderByCustomerResult>
{
    public async Task<GetOrderByCustomerResult> Handle(GetOrderByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = Guid.Parse(query.CustomerId);
        var orders = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null)
            .OrderBy(x => x.OrderName)
            .ToListAsync(cancellationToken);

        var result = MapResult(orders);
        return new GetOrderByCustomerResult(result);
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

    private static List<OrderItems> MapOrderItems(IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new OrderItems(x.Id.Value.ToString(), x.OrderId.Value.ToString(), x.ProductId.Value.ToString(), x.Quantity,
                x.Price.Value)).ToList();
    }
}