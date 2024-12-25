using System.Text.Json;
using BuildingBlocks.Exceptions;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(UpdateOrderDto Order) : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(Ulid.Parse(command.Order.Id));
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var orderDb = await dbContext.Orders.FindAsync([orderId], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        AssertOrder(orderDb, command.Order);
        UpdateOrderWithNewValues(command.Order, orderDb!);
        AddOrderUpdatedEvent(orderDb!);
        var outbox = MapOutbox(orderDb!);

        dbContext.Orders.Update(orderDb!);
        dbContext.Outboxes.Add(outbox);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateOrderResult(true);
    }

    private static void AssertOrder(Domain.Models.Order? orderDb, UpdateOrderDto orderDto)
    {
        if (orderDb is null)
            throw new OrderNotFoundExceptions(Ulid.Parse(orderDto.Id));

        var version = VersionId.FromWeakEtag(orderDto.Version!).Value;
        if (version != orderDb.RowVersion.Value)
            throw new InvalidEtagException(orderDto.Version!);
    }

    private static void AddOrderUpdatedEvent(Domain.Models.Order order)
    {
        order.AddDomainEvent(new OrderUpdatedEvent(order));
    }

    private static void UpdateOrderWithNewValues(UpdateOrderDto orderDto, Domain.Models.Order orderDb)
    {
        var shippingAddress = MapAddress(orderDto.ShippingAddress);
        var billingAddress = MapAddress(orderDto.BillingAddress);
        var payment = MapPayment(orderDto.Payment);
        var orderItems = GetOrderItems(orderDto);

        orderDb.Update(
            OrderName.From(orderDto.OrderName),
            shippingAddress,
            billingAddress,
            payment,
            MapOrderStatus(orderDto.Status),
            versionId: orderDb.RowVersion.Increment(),
            orderItems: orderItems);
    }

    private static List<OrderItem> GetOrderItems(UpdateOrderDto orderDto)
    {
        return orderDto.OrderItems.Select(x =>
            new OrderItem(
                orderId: OrderId.From(Ulid.Parse(orderDto.Id)),
                productId: ProductId.From(Ulid.Parse(x.ProductId!)),
                quantity: x.Quantity!.Value,
                price: Price.From(x.Price!.Value))).ToList();
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

    private static Outbox MapOutbox(Domain.Models.Order order)
    {
        var outbox = new Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.ToIso8601UtcFormat(DateTimeOffset.UtcNow.AddMinutes(2)),
            eventType: EventType.From(nameof(OrderUpdatedEvent)),
            payload: Payload.From(JsonSerializer.Serialize(order)));

        return outbox;
    }
}