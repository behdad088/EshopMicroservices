
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Order.Query.API.IntegrationTests;

public class WebApiContainerFactory : IAsyncLifetime
{
    private const ushort PostgresPort = 5432;
    private const ushort ElasticSearchPort = 9200;
    private readonly IContainer _postgres = new ContainerBuilder()
        .WithImage("postgres")
        .WithPortBinding(5432, true)
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "TestCatalogDb")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PostgresPort))
        .Build();
    
    private readonly IContainer _elasticsearch = new ContainerBuilder()
        .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.1.2")
        .WithPortBinding(9200, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(ElasticSearchPort))
        .Build();
    
    internal string ElasticSearchUri =>
        $"http://{_elasticsearch.Hostname}:{_elasticsearch.GetMappedPublicPort(ElasticSearchPort)}";
    
    internal Uri PostgresUri => new($"http://{_postgres.Hostname}:{_postgres.GetMappedPublicPort(PostgresPort)}");
    internal string PostgresConnectionString =>
        $"Server={_postgres.Hostname};Port={_postgres.GetMappedPublicPort(PostgresPort)};Database=TestCatalogDb;User Id=postgres;Password=postgres";
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync().ConfigureAwait(false);
        await _elasticsearch.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync().ConfigureAwait(false);
        await _elasticsearch.DisposeAsync().ConfigureAwait(false);
    }
}
