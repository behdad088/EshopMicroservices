namespace Order.Command.API.IntegrationTests.Endpoints;

public class TestEndpointBase(Func<Task> clearDatabaseAsync) : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await clearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}