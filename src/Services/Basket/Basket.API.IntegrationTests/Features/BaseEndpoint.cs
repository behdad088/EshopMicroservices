namespace Basket.API.IntegrationTests.Features;

public class BaseEndpoint : IAsyncLifetime
{
    protected readonly ApiSpecification _apiSpecification;
    public BaseEndpoint(ApiSpecification apiSpecification)
    {
        _apiSpecification = apiSpecification;
        _apiSpecification.ResetWireMockServer();
    }
    public async Task InitializeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}