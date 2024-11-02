namespace Order.Command.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(CreateOrderDto Order) : ICommand<CreateOrderResult>;
public record CreateOrderResult(Guid Id);

public class CreateOrderCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = MapOrder(command.Order);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new CreateOrderResult(order.Id.Value);
    }

    private static Domain.Models.Order MapOrder(CreateOrderDto orderDto)
    {
        var order = new Domain.Models.Order().Create(
            id: OrderId.From(orderDto.Id),
            customerId: CustomerId.From(orderDto.CustomerId!.Value),
            orderName: OrderName.From(orderDto.OrderName),
            shippingAddress: MapAddress(orderDto.ShippingAddress),
            billingAddress: MapAddress(orderDto.BillingAddress),
            payment: MapPayment(orderDto.Payment),
            orderItems: orderDto.OrderItems.Select(x =>
                new OrderItem(
                    orderId: OrderId.From(orderDto.Id),
                    productId: ProductId.From(x.ProductId!.Value),
                    quantity: x.Quantity!.Value,
                    price: Price.From(x.Price!.Value))).ToList());

        return order;
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
}