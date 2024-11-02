using Order.Command.Application.Exceptions;

namespace Order.Command.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(string OrderId) : ICommand<DeleteOrderResult>;

public record DeleteOrderResult(bool IsSuccess);

public class DeleteOrderCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(Ulid.Parse(command.OrderId));
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken).ConfigureAwait(false);

        if (order is null)
            throw new OrderNotFoundExceptions(orderId.Value);

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteOrderResult(true);
    }
}