using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.IntegrationTests.ServerGivens;
using IntegrationTests.Common;
using Marten;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace Basket.API.IntegrationTests;

[CollectionDefinition(Name)]
public class GetWebApiContainerFactory : ICollectionFixture<ApiSpecification>
{
    public const string Name = "TestCollection";
}
public class ApiSpecification : IAsyncLifetime
{
    private IDistributedCache? _cache;
    private WireMockServer? _discountWireMockServer;
    private ApiFactory? _factory;

    private HttpClient? _httpClient;
    private PostgresDataSeeder? _postgresDataSeeder;
    private RedisDataSeeder? _redisDataSeeder;  
    private IDocumentStore? _store;
    private readonly WebApiContainerFactory _webApiContainer = new();
    internal HttpClient HttpClient
    {
        get
        { 
            var client = _httpClient ??= _factory!.CreateClient();
            client.ClearDefaultHeaders();
            return client;
        }
    }

    internal PostgresDataSeeder PostgresDataSeeder =>
        _postgresDataSeeder ??= new PostgresDataSeeder(GetDocumentStore());

    internal RedisDataSeeder RedisDataSeeder => _redisDataSeeder ??= new RedisDataSeeder(GetCache());
    internal CancellationToken CreateTimeoutToken(int seconds = 120)
    {
        return new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;
    }

    public void ResetWireMockServer()
    {
        _discountWireMockServer?.Reset();
    }
    
    public async Task InitializeAsync()
    {
        await _webApiContainer.InitializeAsync();
        _discountWireMockServer = StartWireMockServer();
        Environment.SetEnvironmentVariable("Grpc__Discount", _discountWireMockServer.Url);

        _factory = new ApiFactory(
            _webApiContainer.PostgresConnectionString,
            _webApiContainer.RedisConnectionString,
            _webApiContainer.ElasticSearchUri);
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        _cache = _factory.Services.GetRequiredService<IDistributedCache>();
        await Task.CompletedTask;
    }


    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        _httpClient?.Dispose();
        _store?.Advanced.ResetAllData(CreateTimeoutToken());
        _store?.Dispose();
        await _webApiContainer.DisposeAsync();
        _discountWireMockServer?.Stop();
        _discountWireMockServer?.Dispose();
    }

    private static WireMockServer StartWireMockServer()
    {
        var server = WireMockServer.Start(useHttp2: true);
        return server;
    }

    public DiscountGiven CreateDiscountServerGiven()
    {
        return new DiscountGiven(_discountWireMockServer ?? throw new Exception("Failed starting Discount WireMockServer!"));
    }

    internal IDocumentStore GetDocumentStore()
    {
        return _store ??= _factory!.Services.GetRequiredService<IDocumentStore>();
    }

    private IDistributedCache GetCache()
    {
        return _cache ??= _factory!.Services.GetRequiredService<IDistributedCache>();
    }
}