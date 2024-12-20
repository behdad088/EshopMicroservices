﻿using Marten;

namespace Catalog.API.IntegrationTests.Database;

public class DataSeeder(IDocumentStore store)
{
    public async Task SeedDataBaseAsync(Product? product = null)
    {
        var products = product is null ? GetListOfProducts() : [product];

        await using var session = store.LightweightSession();
        session.Store<Product>(products);
        await session.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Product>> GetAllData()
    {
        await using var session = store.LightweightSession();
        var data = await session.Query<Product>().ToListAsync();
        return data;
    }

    public static IEnumerable<Product> GetListOfProducts()
    {
        return
        [
            new()
            {
                Id = Ulid.NewUlid().ToString(),
                Name = "IPhone X",
                Description =
                    "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                ImageFile = "product-1.png",
                Price = 950.00M,
                Category = ["Iphone"]
            },
            new()
            {
                Id = Ulid.NewUlid().ToString(),
                Name = "Samsung 10",
                Description =
                    "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                ImageFile = "product-2.png",
                Price = 840.00M,
                Category = ["Smart Phone"]
            }
        ];
    }
}