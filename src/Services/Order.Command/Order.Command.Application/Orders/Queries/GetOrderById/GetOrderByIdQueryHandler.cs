using BuildingBlocks.Exceptions;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Queries.GetOrderById;

public record GetOrdersByIdQuery(string? Id) : IQuery<GetOrdersByIdResult>;

public record GetOrdersByIdResult(GetOrderByIdDto Order);

public class GetOrderByIdHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByIdQuery, GetOrdersByIdResult>
{
    public async Task<GetOrdersByIdResult> Handle(GetOrdersByIdQuery request, CancellationToken cancellationToken)
    {
        var orderId = Ulid.Parse(request.Id);
        var order = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(OrderId.From(orderId)), cancellationToken);

        if (order is null)
            throw new OrderNotFoundExceptions(orderId);

        var result = MapResult(order);
        return new GetOrdersByIdResult(result);
    }
    
    private static GetOrderByIdDto MapResult(Domain.Models.Order order)
    {
        var result = new GetOrderByIdDto(
            order.Id.Value,
            order.CustomerId.Value,
            order.OrderName.Value,
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            MapPayment(order.Payment),
            order.Status.Value,
            order.RowVersion.Value,
            MapOrderItems(order.OrderItems));

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