using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(UpdateOrderDto Order) : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(command.Order.Id);
        var orderDb = await dbContext.Orders.FindAsync(orderId, cancellationToken).ConfigureAwait(false);

        if (orderDb is null)
            throw new OrderNotFoundExceptions(orderId.Value);

        UpdateOrderWithNewValues(command.Order, orderDb);
        dbContext.Orders.Update(orderDb);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new UpdateOrderResult(true);
    }

    private static void UpdateOrderWithNewValues(UpdateOrderDto orderDto, Domain.Models.Order orderDb)
    {
        var shippingAddress = MapAddress(orderDto.ShippingAddress);
        var billingAddress = MapAddress(orderDto.BillingAddress);
        var payment = MapPayment(orderDto.Payment);

        orderDb.Update(
            OrderName.From(orderDto.OrderName),
            shippingAddress,
            billingAddress,
            payment,
            MapOrderStatus(orderDto.Status));
    }

    private static Address MapAddress(AddressDto addressDto)
    {
        return new Address(
            addressDto.Firstname,
            addressDto.Lastname,
            addressDto.EmailAddress,
            addressDto.AddressLine,
            addressDto.Country,
            addressDto.State,
            addressDto.ZipCode);
    }

    private static Payment MapPayment(PaymentDto paymentDto)
    {
        return new Payment(
            paymentDto.CardName,
            paymentDto.CardName,
            paymentDto.Expiration,
            paymentDto.Cvv,
            paymentDto.PaymentMethod);
    }

    private static OrderStatus MapOrderStatus(string status)
    {
        return status switch
        {
            OrderStatusDto.Pending => OrderStatus.Pending,
            OrderStatusDto.Completed => OrderStatus.Completed,
            OrderStatusDto.Cancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }
}