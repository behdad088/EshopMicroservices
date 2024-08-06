using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Basket.API.IntegrationTests;

public class WebApiContainerFactory : IAsyncLifetime
{
    private const ushort PostgresPort = 5432;
    private readonly IContainer _postgres = new ContainerBuilder()
        .WithImage("postgres")
        .WithPortBinding(5432, true)
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "TestCatalogDb")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PostgresPort))
        .Build();

    internal Uri PostgresUri => new($"http://{_postgres.Hostname}:{_postgres.GetMappedPublicPort(PostgresPort)}");

    internal string PostgresConnectionString =>
        $"Server={_postgres.Hostname};Port={_postgres.GetMappedPublicPort(PostgresPort)};Database=TestCatalogDb;User Id=postgres;Password=postgres";

    private const ushort RedisPort = 6379;
    private readonly IContainer _redis = new ContainerBuilder()
        .WithImage("redis:alpine")
        .WithPortBinding(6379, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(RedisPort))
        .Build();

    internal string RedisConnectionString => $"{_redis.Hostname}:{_redis.GetMappedPublicPort(RedisPort)}";
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync().ConfigureAwait(false);
        await _redis.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync().ConfigureAwait(false);
        await _redis.DisposeAsync().ConfigureAwait(false);
    }
}

[CollectionDefinition(Name)]
public class GetWebApiContainerFactory : ICollectionFixture<WebApiContainerFactory>
{
    public const string Name = "WebApiContainerFactory";
}