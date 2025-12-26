namespace Order.Command.API.IntegrationTests.Given.SqlGiven;

public class SqlDbGiven(IApplicationDbContext dbContext)
{
    
    public async Task AnOrder(Action<OrderConfiguration>? configure = null)
    {
        var order = new OrderConfiguration();
        configure?.Invoke(order);
        dbContext.Orders.Add(order.ToOrderDb());
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
    
    public async Task AnOutbox(Action<OutboxConfiguration>? configure = null)
    {
        var outbox = new OutboxConfiguration();
        configure?.Invoke(outbox);

        dbContext.Outboxes.Add(outbox.ToOutboxDb());
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}