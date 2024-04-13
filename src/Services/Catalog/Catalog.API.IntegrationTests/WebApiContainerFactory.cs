using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Catalog.API.IntegrationTests
{
    public class WebApiContainerFactory : IAsyncLifetime
    {
        private const ushort postgresPort = 5432;
        private readonly IContainer _postgres = new ContainerBuilder()
            .WithImage("postgres")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "TestCatalogDb")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(postgresPort))
            .Build();

        internal Uri PostgresUri => new($"http://{_postgres.Hostname}:{_postgres.GetMappedPublicPort(postgresPort)}");

        internal string PostgresConnectionString =>
            $"Server={_postgres.Hostname};Port={_postgres.GetMappedPublicPort(postgresPort)};Database=TestCatalogDb;User Id=postgres;Password=postgres";

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync().ConfigureAwait(false);
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync().ConfigureAwait(false);
        }
    }

    [CollectionDefinition(Name)]
    public class GetWebApiContainerFactory : ICollectionFixture<WebApiContainerFactory>
    {
        public const string Name = "WebApiContainerFactory";
    }
}
