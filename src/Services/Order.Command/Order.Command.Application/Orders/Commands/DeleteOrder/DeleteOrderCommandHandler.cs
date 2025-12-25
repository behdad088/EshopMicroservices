using eshop.Shared.CQRS.Command;
using Npgsql;

namespace Order.Command.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(string? CustomerId, string? OrderId, string? Version) : ICommand<Result>;

public abstract record Result
{
    public record Succeed : Result;
    public record InvalidEtag(string Etag) : Result;
    public record OrderNotFound(string Id) : Result;
}

public class DeleteOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteOrderCommand, Result>
{
    private readonly ILogger _logger = Log.ForContext<DeleteOrderCommandHandler>();
    
    public async Task<Result> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            using var _ = LogContext.PushProperty(LogProperties.ETag, command.Version);
            _logger.Information("Delete Order Command.");
            var customerId = CustomerId.From(Guid.Parse(command.CustomerId!));
            var orderId = OrderId.From(Ulid.Parse(command.OrderId));
            var version = VersionId.FromWeakEtag(command.Version!).Value;

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var order = await dbContext.Orders
                .FirstOrDefaultAsync(x => x.Id.Equals(orderId) && x.DeleteDate == null, cancellationToken)
                .ConfigureAwait(false);

            var assertResult = IsOrderValid(order, command.OrderId!, version);
            if (assertResult is not null)
                return assertResult;
            
            order!.Delete(order.RowVersion.Increment());
            var outbox = MapOutbox(customerId, order);

            dbContext.Orders.Update(order);
            dbContext.Outboxes.Add(outbox);

            AddOrderDeletedEvent(customerId, order);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            _logger.Information("Successfully deleted Order.");
            return new Result.Succeed();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505")
            {
                _logger.Error("Invalid etag.");
                return new Result.InvalidEtag(command.Version!);
            }

            throw;
        }
    }

    private Result? IsOrderValid(Domain.Models.Order? orderDb, string orderId, int version)
    {
        if (orderDb is null)
        {
            _logger.Error("No order found.");
            return new Result.OrderNotFound(orderId);
        }
            

        if (version != orderDb.RowVersion.Value)
        {
            _logger.Error("Order version doesn't match order version.");
            return new Result.InvalidEtag(version.ToString());
        }

        return null;
    }

    private static void AddOrderDeletedEvent(CustomerId customerId, Domain.Models.Order order)
    {
        var @event = MapOrderDeletedEvent(customerId, order);
        order.AddDomainEvent(@event);
    }

    private static Domain.Models.Outbox MapOutbox(CustomerId customerId, Domain.Models.Order order)
    {
        var payload = Payload.Serialize(MapOrderDeletedEvent(customerId, order));
        var outbox = new Domain.Models.Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            customerId: customerId,
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.InTwoMinutes(),
            eventType: EventType.From(nameof(OrderDeletedEvent)),
            payload: payload);

        return outbox;
    }

    private static OrderDeletedEvent MapOrderDeletedEvent(CustomerId customerId, Domain.Models.Order order)
    {
        return new OrderDeletedEvent(order.Id.Value, customerId.Value, order.DeleteDate!.Value, order.RowVersion.Value);
    }
}