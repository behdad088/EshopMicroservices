using eshop.Shared.CQRS.Query;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Queries.GetOrderById;

public record GetOrdersByIdQuery(string? Id, string? CustomerId) : IQuery<GetOrdersByIdResult>;

public record GetOrdersByIdResult(GetOrderByIdResponse Order);

public class GetOrderByIdHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByIdQuery, GetOrdersByIdResult>
{
    private readonly ILogger _logger = Log.ForContext<GetOrderByIdHandler>();
    public async Task<GetOrdersByIdResult> Handle(GetOrdersByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Get orders by id.");
        var orderId = Ulid.Parse(request.Id);
        var customerId = Guid.Parse(request.CustomerId!);
        var order = await dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(OrderId.From(orderId)) && x.CustomerId.Equals(CustomerId.From(customerId)) && x.DeleteDate == null, cancellationToken);

        if (order is null)
            throw new OrderNotFoundExceptions(orderId);

        var result = MapResult(order);
        _logger.Information("Successfully retrieved orders by id.");
        return new GetOrdersByIdResult(result);
    }

    private static GetOrderByIdResponse MapResult(Domain.Models.Order order)
    {
        var result = new GetOrderByIdResponse(
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

    private static GetOrderByIdResponse.AddressResponse MapAddress(Address address)
    {
        return new GetOrderByIdResponse.AddressResponse(
            address.FirstName,
            address.LastName,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static GetOrderByIdResponse.PaymentResponse MapPayment(Payment payment)
    {
        return new GetOrderByIdResponse.PaymentResponse(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.CVV,
            payment.PaymentMethod);
    }

    private static List<GetOrderByIdResponse.OrderItemResponse> MapOrderItems(
        IReadOnlyCollection<OrderItem> orderItems)
    {
        return orderItems.Select(x =>
            new GetOrderByIdResponse.OrderItemResponse(
                x.ProductId.Value.ToString(),
                x.Quantity,
                x.Price.Value)).ToList();
    }
}