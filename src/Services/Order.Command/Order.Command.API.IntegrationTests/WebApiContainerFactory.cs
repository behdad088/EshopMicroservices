using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.RabbitMq;

namespace Order.Command.API.IntegrationTests;

public class WebApiContainerFactory : IAsyncLifetime
{
    private const ushort RmqPort = 5672;
    private const ushort RmqUiPort = 5672;
    private const string RmqUsername = "test";
    private const string RmqPassword = "test";
    private const ushort ElasticSearchPort = 9200;
    
    private const ushort PostgresPort = 5432;
    private readonly IContainer _postgres = new ContainerBuilder()
        .WithImage("postgres")
        .WithPortBinding(5432, true)
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "TestOrderDb")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PostgresPort))
        .Build();

    internal string PostgresConnectionString =>
        $"Server={_postgres.Hostname};Port={_postgres.GetMappedPublicPort(PostgresPort)};Database=TestOrderDb;User Id=postgres;Password=postgres";
    
    internal string ElasticSearchUri =>
        $"http://{_elasticsearch.Hostname}:{_elasticsearch.GetMappedPublicPort(ElasticSearchPort)}";
    
    private readonly RabbitMqContainer _rmq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername(RmqUsername)
        .WithPassword(RmqPassword)
        .WithPortBinding(RmqPort)
        .WithPortBinding(RmqUiPort)
        .Build();
    
    private readonly IContainer _elasticsearch = new ContainerBuilder()
        .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.1.2")
        .WithPortBinding(9200, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(ElasticSearchPort))
        .Build();
    
    internal RmqConfiguration RmqConfiguration => new(_rmq.Hostname, RmqUsername, RmqPassword);
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync().ConfigureAwait(false);
        await _rmq.StartAsync().ConfigureAwait(false);
        await _elasticsearch.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync().ConfigureAwait(false);
        await _rmq.DisposeAsync().ConfigureAwait(false);
        await _elasticsearch.DisposeAsync().ConfigureAwait(false);
    }
}

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<WebApiContainerFactory>
{
    public const string Name = "TestCollection";
}

public record RmqConfiguration(string Uri, string Username, string Password);