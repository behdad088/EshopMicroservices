using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(OrderDto OrderDto) : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);
    
public class UpdateOrderCommandHandler(IApplicationDbContext dbContext) 
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(command.OrderDto.Id);
        var orderDb = await dbContext.Orders.FindAsync(orderId, cancellationToken).ConfigureAwait(false);

        if (orderDb is null)
            throw new OrderNotFoundExceptions(orderId.Value);

        UpdateOrderWithNewValues(command.OrderDto, orderDb);
        dbContext.Orders.Update(orderDb);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdateOrderResult(true);
    }
    
    private static void UpdateOrderWithNewValues(OrderDto orderDto, Domain.Models.Order orderDb)
    {
        var shippingAddress = MapAddress(orderDto.ShippingAddress);
        var billingAddress = MapAddress(orderDto.BillingAddress);
        var payment = MapPayment(orderDto.Payment);

        orderDb.Update(
            orderName: OrderName.From(orderDto.OrderName),
            shippingAddress: shippingAddress,
            billingAddress: billingAddress,
            payment: payment,
            orderStatus: MapOrderStatus(orderDto.Status));
    }

    private static Address MapAddress(AddressDto addressDto)
        => new(
            firstName: addressDto.Firstname,
            lastName: addressDto.Lastname,
            emailAddress: addressDto.EmailAddress,
            addressLine: addressDto.AddressLine,
            country: addressDto.Country,
            state: addressDto.State,
            zipCode: addressDto.ZipCode);

    private static Payment MapPayment(PaymentDto paymentDto) =>
        new (
            cardName: paymentDto.CardName,
            cardNumber: paymentDto.CardName,
            expiration: paymentDto.Expiration,
            cvv: paymentDto.Cvv,
            paymentMethod: paymentDto.PaymentMethod);

    private static OrderStatus MapOrderStatus(string status) => status switch
    {
        OrderStatusDto.Draft => OrderStatus.Draft,
        OrderStatusDto.Pending => OrderStatus.Pending,
        OrderStatusDto.Completed => OrderStatus.Completed,
        OrderStatusDto.Cancelled => OrderStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}