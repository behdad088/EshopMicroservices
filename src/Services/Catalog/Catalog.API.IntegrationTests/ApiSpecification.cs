using IntegrationTests.Common;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.IntegrationTests;

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<ApiSpecification>
{
    public const string Name = "TestCollection";
}

public class ApiSpecification : IAsyncLifetime
{
    private ApiFactory? _factory;
    private IDocumentStore? _store;
    private DataSeeder? _dataSeeder;
    private readonly WebApiContainerFactory _webApiContainer = new();
    
    public async Task InitializeAsync()
    {
        await _webApiContainer.InitializeAsync();
        _factory = new ApiFactory(_webApiContainer.PostgresConnectionString);
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        await Task.CompletedTask;
    }

    private HttpClient? _httpClient;
    internal HttpClient HttpClient
    {
        get
        {
            var client = _httpClient ??= _factory!.CreateClient();
            client.ClearDefaultHeaders();
            return client;
        }
    }

    internal IDocumentStore GetDocumentStore() => _store ??= _factory!.Services.GetRequiredService<IDocumentStore>();
    internal DataSeeder DataSeeder => _dataSeeder ??= new DataSeeder(GetDocumentStore());

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        _httpClient?.Dispose();
        _store?.Advanced.ResetAllData();
        _store?.Dispose();
        await _webApiContainer.DisposeAsync();
    }
}