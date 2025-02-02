using System.Diagnostics;
using BuildingBlocks.Exceptions;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Queries.GetOrderById;

public record GetOrdersByIdQuery(string? Id) : IQuery<GetOrdersByIdResult>;

public record GetOrdersByIdResult(GetOrderByIdParameter Order);

public class GetOrderByIdHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByIdQuery, GetOrdersByIdResult>
{
    public async Task<GetOrdersByIdResult> Handle(GetOrdersByIdQuery request, CancellationToken cancellationToken)
    {
        var orderId = Ulid.Parse(request.Id);
        var order = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(OrderId.From(orderId)) && x.DeleteDate == null, cancellationToken);

        if (order is null)
            throw new OrderNotFoundExceptions(orderId);

        var result = MapResult(order);
        return new GetOrdersByIdResult(result);
    }

    private static GetOrderByIdParameter MapResult(Domain.Models.Order order)
    {
        var result = new GetOrderByIdParameter(
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