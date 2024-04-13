using Catalog.API.Data;
using Catalog.API.Models;
using Marten;
using Marten.Schema;
using Polly;
using Serilog;
using System.Net.Sockets;

namespace Catalog.API.IntegrationTests.Database;

public class TestDatabaseMigration : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        var logger = Log.Logger.ForContext<TestDatabaseMigration>();

        try
        {
            logger.Information("Migrate test postresql database started.");
            var policy = Policy.Handle<SocketException>()
               .Or<Marten.Exceptions.MartenCommandException>()
               .WaitAndRetryAsync(retryCount: 7, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
               {
                   logger.Error(ex, "An error occurred while migrating the postresql database, failed trying after {TimeOut}s", $"{time.TotalSeconds:n1}");
               });

            await policy.ExecuteAsync(async () =>
            {
                using var session = store.LightweightSession();
                if (await session.Query<Product>().AnyAsync())
                    return;

                session.Store<Product>(GetPreConfiguredProducts());
                await session.SaveChangesAsync();
            });
        }
        catch (Marten.Exceptions.MartenCommandException ex)
        {
            logger.Error(ex, "An error occurred while migrating the postresql database.");
        }
    }

    private static IEnumerable<Product> GetPreConfiguredProducts() =>
    [
        new()
        {
            Id = Guid.Parse("11d837be-a763-488d-a52e-9ff21ac9d1c2"),
            Name = "IPhone X",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFile = "product-1.png",
            Price = 950.00M,
            Category = ["Smart Phone"]
        },
        new()
        {
            Id = Guid.Parse("c92eba47-06a6-4730-82d4-8f36789c824a"),
            Name = "Samsung 10",
            Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
            ImageFile = "product-2.png",
            Price = 840.00M,
            Category = ["Smart Phone"]
        }
    ];
}