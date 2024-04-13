using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.IntegrationTests;

[Collection(GetWebApiContainerFactory.Name)]
public class ApiSpecification(WebApiContainerFactory webApiContainer) : IAsyncLifetime
{
    private ApiFactory? _factory;
    private IDocumentStore? _store;
    private DataSeeder? _dataSeeder;

    public async Task InitializeAsync()
    {
        _factory = new ApiFactory(webApiContainer.PostgresConnectionString);
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        await Task.CompletedTask;
    }

    private HttpClient? _httpClient;
    internal HttpClient HtpClient => _httpClient ??= _factory!.CreateClient();
    internal IDocumentStore GetDocumentStore() => _store ??= _factory!.Services.GetRequiredService<IDocumentStore>();
    internal DataSeeder DataSeeder => _dataSeeder ??= new DataSeeder(GetDocumentStore());

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        _httpClient?.Dispose();
        _store?.Advanced.ResetAllData();
        _store?.Dispose();
    }
}