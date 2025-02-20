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
    
    private const ushort MssqlPort = 1433;
    private const string MssqlPassword = "BeH007826790";
    private readonly IContainer _mssql = new ContainerBuilder()
        .WithImage("mcr.microsoft.com/mssql/server")
        .WithPortBinding(MssqlPort)
        .WithEnvironment("SA_PASSWORD", MssqlPassword)
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilPortIsAvailable(MssqlPort)
            .UntilMessageIsLogged("SQL Server is now ready for client connections"))
        .Build();

    internal string MssqlConnectionString =>
        $"Server={_mssql.Hostname};Database=OrderDb;User Id=sa;Password={MssqlPassword};Trusted_Connection=False;TrustServerCertificate=True";
    
    private readonly RabbitMqContainer _rmq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername(RmqUsername)
        .WithPassword(RmqPassword)
        .WithPortBinding(RmqPort)
        .WithPortBinding(RmqUiPort)
        .Build();
    
    internal RmqConfiguration RmqConfiguration => new(_rmq.Hostname, RmqUsername, RmqPassword);
    
    public async Task InitializeAsync()
    {
        await _mssql.StartAsync().ConfigureAwait(false);
        await _rmq.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _mssql.DisposeAsync().ConfigureAwait(false);
        await _rmq.DisposeAsync().ConfigureAwait(false);
    }
}

public record RmqConfiguration(string Uri, string Username, string Password);

[CollectionDefinition(Name)]
public class GetWebApiContainerFactory : ICollectionFixture<WebApiContainerFactory>
{
    public const string Name = "WebApiContainerFactory";
}