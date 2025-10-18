using Marten;
using Order.Query.Data.Events;
using Order.Query.Data.Views.OrderView;

namespace Order.Query.EventProcessor.IntegrationTests.Given.SqlGiven;

public class SqlGiven(IDocumentStore store)
{
    public async Task OrderViewConfigurationGiven(Action<OrderViewConfiguration>? configuration = null)
    {
        var orderView = new OrderViewConfiguration();
        configuration?.Invoke(orderView);
        
        await using var session = store.LightweightSession();
        session.Store<OrderView>(orderView.ToDbModel());
        await session.SaveChangesAsync();
    }

    public async Task EventStreamConfigurationGiven(Action<EventStreamConfiguration>? configuration = null)
    {
        var eventStream = new EventStreamConfiguration();
        configuration?.Invoke(eventStream);
        
        await using var session = store.LightweightSession();
        session.Store<EventStream>(eventStream.ToDbModel());
        await session.SaveChangesAsync();
    }
}