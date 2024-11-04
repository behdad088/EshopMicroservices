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
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    private IDistributedCache? _cache;
    private WireMockServer? _discountWireMockServer;
    private ApiFactory? _factory;

    private HttpClient? _httpClient;
    private PostgresDataSeeder? _postgresDataSeeder;
    private RedisDataSeeder? _redisDataSeeder;
    private IDocumentStore? _store;
    internal HttpClient HttpClient => _httpClient ??= _factory!.CreateClient();

    internal PostgresDataSeeder PostgresDataSeeder =>
        _postgresDataSeeder ??= new PostgresDataSeeder(GetDocumentStore());

    internal RedisDataSeeder RedisDataSeeder => _redisDataSeeder ??= new RedisDataSeeder(GetCache());
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;

    public async Task InitializeAsync()
    {
        _discountWireMockServer = StartWireMockServer();
        Environment.SetEnvironmentVariable("Grpc__Discount", _discountWireMockServer.Url);

        _factory = new ApiFactory(webApiContainer.PostgresConnectionString, webApiContainer.RedisConnectionString);
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        _cache = _factory.Services.GetRequiredService<IDistributedCache>();
        await Task.CompletedTask;
    }


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