using BuildingBlocks.Exceptions;
using Microsoft.Data.SqlClient;
using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(string? OrderId, string? Version) : ICommand<DeleteOrderResult>;

public record DeleteOrderResult(bool IsSuccess);

public class DeleteOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var orderId = OrderId.From(Ulid.Parse(command.OrderId));
            var version = VersionId.FromWeakEtag(command.Version!).Value;

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var order = await dbContext.Orders
                .FirstOrDefaultAsync(x => x.Id.Equals(orderId) && x.DeleteDate == null, cancellationToken)
                .ConfigureAwait(false);

            AssertOrder(order, command.OrderId!, version);
            order!.Delete(order.RowVersion.Increment());
            var outbox = MapOutbox(order);

            dbContext.Orders.Update(order);
            dbContext.Outboxes.Add(outbox);

            AddOrderDeletedEvent(order);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new DeleteOrderResult(true);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                throw new InvalidEtagException(command.Version!);
            }

            throw;
        }
    }

    private static void AssertOrder(Domain.Models.Order? orderDb, string orderId, int version)
    {
        if (orderDb is null)
            throw new OrderNotFoundExceptions(Ulid.Parse(orderId));

        if (version != orderDb.RowVersion.Value)
            throw new InvalidEtagException(version);
    }

    private static void AddOrderDeletedEvent(Domain.Models.Order order)
    {
        var @event = MapOrderDeletedEvent(order);
        order.AddDomainEvent(@event);
    }

    private static Domain.Models.Outbox MapOutbox(Domain.Models.Order order)
    {
        var payload = Payload.Serialize(MapOrderDeletedEvent(order));
        var outbox = new Domain.Models.Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.InTwoMinutes(),
            eventType: EventType.From(nameof(OrderDeletedEvent)),
            payload: payload);

        return outbox;
    }

    private static OrderDeletedEvent MapOrderDeletedEvent(Domain.Models.Order order)
    {
        return new OrderDeletedEvent(order.Id.Value, order.DeleteDate!.Value, order.RowVersion.Value);
    }
}