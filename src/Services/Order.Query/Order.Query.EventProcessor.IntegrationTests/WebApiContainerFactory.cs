using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.RabbitMq;

namespace Order.Query.EventProcessor.IntegrationTests;

public class WebApiContainerFactory : IAsyncLifetime
{
    private const ushort RmqPort = 5672;
    private const ushort RmqUiPort = 5672;
    private const string RmqUsername = "test";
    private const string RmqPassword = "test";
    
    private readonly RabbitMqContainer _rmq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername(RmqUsername)
        .WithPassword(RmqPassword)
        .WithPortBinding(RmqPort)
        .WithPortBinding(RmqUiPort)
        .Build();
    
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

    
    internal RmqConfiguration RmqConfiguration => new(_rmq.Hostname, RmqUsername, RmqPassword);
    
    public async Task InitializeAsync()
    {
        await _rmq.StartAsync().ConfigureAwait(false);
        await _postgres.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _rmq.DisposeAsync().ConfigureAwait(false);
        await _postgres.DisposeAsync().ConfigureAwait(false);
    }
}

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<WebApiContainerFactory>
{
    public const string Name = "TestCollection";
}

public record RmqConfiguration(string Uri, string Username, string Password);