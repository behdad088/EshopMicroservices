namespace Order.Command.API.IntegrationTests.Given.SqlGiven;

public class SqlDbGiven(IApplicationDbContext dbContext)
{
    
    public async Task AnOrder(Action<OrderConfiguration>? configure = null)
    {
        var order = new OrderConfiguration();
        configure?.Invoke(order);

        dbContext.Orders.Add(order.ToOrderDb());
        await dbContext.SaveChangesAsync(default);
    }
}