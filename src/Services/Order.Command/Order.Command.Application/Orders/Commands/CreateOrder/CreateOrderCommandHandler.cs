using System.Text.Json;

namespace Order.Command.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderDto OrderDto) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Ulid Id);

public class CreateOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = MapOrder(command.OrderDto);
        var outbox = MapOutbox(order);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Orders.Add(order);
        dbContext.Outboxes.Add(outbox);

        AddOrderCreatedEvent(order);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken);
        return new CreateOrderResult(order.Id.Value);
    }

    private static Domain.Models.Order MapOrder(OrderDto orderDtoDto)
    {
        var order = new Domain.Models.Order().Create(
            id: OrderId.From(Ulid.Parse(orderDtoDto.Id)),
            customerId: CustomerId.From(Guid.Parse(orderDtoDto.CustomerId!)),
            orderName: OrderName.From(orderDtoDto.OrderName!),
            shippingAddress: MapAddress(orderDtoDto.ShippingAddress!),
            billingAddress: MapAddress(orderDtoDto.BillingAddress!),
            payment: MapPayment(orderDtoDto.OrderPayment!),
            orderItems: orderDtoDto.OrderItems!.Select(x =>
                new OrderItem(
                    orderId: OrderId.From(Ulid.Parse(orderDtoDto.Id)),
                    productId: ProductId.From(Ulid.Parse(x.ProductId!)),
                    quantity: x.Quantity!.Value,
                    price: Price.From(x.Price!.Value))).ToList());

        return order;
    }

    private static Address MapAddress(OrderDto.Address addressDto)
        => new(
            firstName: addressDto.Firstname,
            lastName: addressDto.Lastname,
            emailAddress: addressDto.EmailAddress,
            addressLine: addressDto.AddressLine,
            country: addressDto.Country,
            state: addressDto.State,
            zipCode: addressDto.ZipCode);

    private static Payment MapPayment(OrderDto.Payment paymentDto) =>
        new(
            cardName: paymentDto.CardName,
            cardNumber: paymentDto.CardName,
            expiration: paymentDto.Expiration,
            cvv: paymentDto.Cvv,
            paymentMethod: paymentDto.PaymentMethod);

    private static void AddOrderCreatedEvent(Domain.Models.Order order)
    {
        order.AddDomainEvent(new OrderCreatedEvent(order));
    }

    private static Outbox MapOutbox(Domain.Models.Order order)
    {
        var outbox = new Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.ToIso8601UtcFormat(DateTimeOffset.UtcNow.AddMinutes(2)),
            eventType: EventType.From(nameof(OrderCreatedEvent)),
            payload: Payload.From(JsonSerializer.Serialize(order)));

        return outbox;
    }
}