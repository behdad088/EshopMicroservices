using eshop.Shared.CQRS.Command;
using Npgsql;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(UpdateOrderParameter Order) : ICommand<Result>;

public abstract record Result
{
    public record Succeed : Result;
    public record InvalidEtag(string Etag) : Result;
    public record OrderNotFound(string Id) : Result;
    
}

public class UpdateOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateOrderCommand, Result>
{
    private readonly ILogger _logger = Log.ForContext<UpdateOrderCommandHandler>();
    public async Task<Result> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            using var _ = LogContext.PushProperty(LogProperties.ETag, command.Order.Version);
            
            _logger.Information("Update Order Command.");
            var customerId = CustomerId.From(Guid.Parse(command.Order.CustomerId!));
            var orderId = OrderId.From(Ulid.Parse(command.Order.Id));
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var orderDb = await dbContext.Orders.FindAsync([orderId], cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var assertResult = IsOrderValid(orderDb, command.Order);
            if (assertResult is not null)
                return assertResult;
            
            UpdateOrderWithNewValues(command.Order, orderDb!);
            AddOrderUpdatedEvent(orderDb!);
            var outbox = MapOutbox(customerId, orderDb!);

            dbContext.Orders.Update(orderDb!);
            dbContext.Outboxes.Add(outbox);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger.Information("Successfully updated order.");
            return new Result.Succeed();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                _logger.Error("Invalid order version.");
                return new Result.InvalidEtag(command.Order.Version!);
            }

            throw;
        }
    }

    private Result? IsOrderValid(Domain.Models.Order? orderDb, UpdateOrderParameter orderParameter)
    {
        if (orderDb is null)
        {
            _logger.Information("Order not found.");
            return new Result.OrderNotFound(orderParameter.Id!);
        }

        var version = VersionId.FromWeakEtag(orderParameter.Version!).Value;
        if (version != orderDb.RowVersion.Value)
        {
            _logger.Information("Order version doesn't match order version.");
            return new Result.InvalidEtag(orderParameter.Version!);
        }
        
        return null;
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