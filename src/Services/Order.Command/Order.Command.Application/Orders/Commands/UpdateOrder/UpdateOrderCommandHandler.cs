using BuildingBlocks.Exceptions;
using eshop.Shared.CQRS.Command;
using Microsoft.Data.SqlClient;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(UpdateOrderParameter Order) : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, UpdateOrderResult>
{
    public async Task<UpdateOrderResult> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var customerId = CustomerId.From(Guid.Parse(command.Order.CustomerId!));
            var orderId = OrderId.From(Ulid.Parse(command.Order.Id));
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var orderDb = await dbContext.Orders.FindAsync([orderId], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            AssertOrder(orderDb, command.Order);
            UpdateOrderWithNewValues(command.Order, orderDb!);
            AddOrderUpdatedEvent(orderDb!);
            var outbox = MapOutbox(customerId, orderDb!);

            dbContext.Orders.Update(orderDb!);
            dbContext.Outboxes.Add(outbox);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new UpdateOrderResult(true);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                throw new InvalidEtagException(command.Order.Version!);
            }

            throw;
        }
    }

    private static void AssertOrder(Domain.Models.Order? orderDb, UpdateOrderParameter orderParameter)
    {
        if (orderDb is null)
            throw new OrderNotFoundExceptions(Ulid.Parse(orderParameter.Id));

        var version = VersionId.FromWeakEtag(orderParameter.Version!).Value;
        if (version != orderDb.RowVersion.Value)
            throw new InvalidEtagException(orderParameter.Version!);
    }

    private static void AddOrderUpdatedEvent(Domain.Models.Order order)
    {
        order.AddDomainEvent(order.ToOrderUpdatedEvent());
    }

    private static void UpdateOrderWithNewValues(
        UpdateOrderParameter orderParameter,
        Domain.Models.Order orderDb)
    {
        var shippingAddress = MapAddress(orderParameter.ShippingAddress!);
        var billingAddress = MapAddress(orderParameter.BillingAddress!);
        var payment = MapPayment(orderParameter.OrderPayment!);
        var orderItems = GetOrderItems(orderParameter);

        orderDb.Update(
            OrderName.From(orderParameter.OrderName!),
            shippingAddress,
            billingAddress,
            payment,
            MapOrderStatus(orderParameter.Status!),
            versionId: VersionId.FromWeakEtag(orderParameter.Version!).Increment(),
            orderItems: orderItems);
    }

    private static List<OrderItem> GetOrderItems(UpdateOrderParameter orderParameter)
    {
        return orderParameter.OrderItems!.Select(x =>
            new OrderItem(
                orderId: OrderId.From(Ulid.Parse(orderParameter.Id)),
                productId: ProductId.From(Ulid.Parse(x.ProductId!)),
                quantity: x.Quantity!.Value,
                price: Price.From(x.Price!.Value))).ToList();
    }

    private static Address MapAddress(UpdateOrderParameter.Address addressParameter)
    {
        return new Address(
            addressParameter.Firstname!,
            addressParameter.Lastname!,
            addressParameter.EmailAddress!,
            addressParameter.AddressLine!,
            addressParameter.Country!,
            addressParameter.State!,
            addressParameter.ZipCode!);
    }

    private static Payment MapPayment(UpdateOrderParameter.Payment paymentParameter)
    {
        return new Payment(
            paymentParameter.CardName!,
            paymentParameter.CardNumber!,
            paymentParameter.Expiration!,
            paymentParameter.Cvv!,
            paymentParameter.PaymentMethod!.Value);
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

    private static Domain.Models.Outbox MapOutbox(CustomerId customerId, Domain.Models.Order order)
    {
        var outbox = new Domain.Models.Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            customerId: customerId,
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.InTwoMinutes(),
            eventType: EventType.From(nameof(OrderUpdatedEvent)),
            payload: Payload.Serialize(order.ToOrderUpdatedEvent()));

        return outbox;
    }
}