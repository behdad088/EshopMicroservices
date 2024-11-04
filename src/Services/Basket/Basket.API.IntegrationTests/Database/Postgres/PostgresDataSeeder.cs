using Basket.API.Models;
using Marten;

namespace Basket.API.IntegrationTests.Database.Postgres;

public class PostgresDataSeeder(IDocumentStore store)
{
    public async Task SeedDatabaseAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();
        session.Store(shoppingCart);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShoppingCart?> GetBasketAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var session = store.LightweightSession();
        var basket = await session.LoadAsync<ShoppingCart>(username, cancellationToken).ConfigureAwait(false);
        return basket;
    }
}