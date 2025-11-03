using Marten;
using Order.Query.Features.OrderView;

namespace Order.Query.API.IntegrationTests.Given.DbGiven;

public class DbGiven(IDocumentStore store)
{
    public async Task OrderViewConfigurationGiven(Action<OrderViewConfiguration>? configuration = null)
    {
        var orderView = new OrderViewConfiguration();
        configuration?.Invoke(orderView);
        
        await using var session = store.LightweightSession();
        session.Store<OrderView>(orderView.ToDbModel());
        await session.SaveChangesAsync();
    }
}