using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.IntegrationTests.ServerGivens;
using Marten;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace Basket.API.IntegrationTests;

internal class ApiSpecification(WebApiContainerFactory webApiContainer) : IAsyncLifetime
{
    private ApiFactory? _factory;
    private IDocumentStore? _store;
    private IDistributedCache? _cache;
    private PostgresDataSeeder? _postgresDataSeeder;
    private RedisDataSeeder? _redisDataSeeder;
    private WireMockServer? _discountWireMockServer;
        
    public async Task InitializeAsync()
    {
        _discountWireMockServer = StartWireMockServer();
        Environment.SetEnvironmentVariable("Grpc__Discount", _discountWireMockServer.Url);
        
        _factory = new ApiFactory(webApiContainer.PostgresConnectionString, webApiContainer.RedisConnectionString);
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        _cache =  _factory.Services.GetRequiredService<IDistributedCache>();
        await Task.CompletedTask;
    }
    
    private static WireMockServer StartWireMockServer()
    {
        var server = WireMockServer.Start(useHttp2: true);
        return server;
    }
    
    public DiscountGiven CreateDiscountServerGiven() =>
        new (_discountWireMockServer ?? throw new Exception("Failed starting Discount WireMockServer!"));
    
    private HttpClient? _httpClient;
    internal HttpClient HttpClient => _httpClient ??= _factory!.CreateClient();
    
    internal IDocumentStore GetDocumentStore() => _store ??= _factory!.Services.GetRequiredService<IDocumentStore>();
    internal PostgresDataSeeder PostgresDataSeeder => _postgresDataSeeder ??= new PostgresDataSeeder(GetDocumentStore());

    private IDistributedCache GetCache() => _cache ??= _factory!.Services.GetRequiredService<IDistributedCache>();
    internal RedisDataSeeder RedisDataSeeder => _redisDataSeeder ??= new RedisDataSeeder(GetCache());
    
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token; 

    
    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        _httpClient?.Dispose();
        _store?.Advanced.ResetAllData(CancellationToken);
        _store?.Dispose();
        
        _discountWireMockServer?.Stop();
        _discountWireMockServer?.Dispose();
    }
}