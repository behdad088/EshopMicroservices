using Basket.API.Models;
using Marten;

namespace Basket.API.IntegrationTests.Database.Postgres;

public class PostgresDataSeeder(IDocumentStore store)
{
    public async Task SeedDataBaseAsync(ShoppingCart shoppingCart)
    {
        await using var session = store.LightweightSession();
        session.Store<ShoppingCart>(shoppingCart);
        await session.SaveChangesAsync();
    }

}