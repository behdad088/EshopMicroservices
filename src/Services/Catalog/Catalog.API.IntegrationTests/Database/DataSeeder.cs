using Catalog.API.Models;
using Marten;

namespace Catalog.API.IntegrationTests.Database
{
    public class DataSeeder(IDocumentStore store)
    {
        public async Task SeedDataBaseAsync(Product? product = null)
        {
            var products = product is null ? GetListOfProducts() : [product];

            await using var session = store.LightweightSession();
            session.Store<Product>(products);
            await session.SaveChangesAsync();
        }


        public static IEnumerable<Product> GetListOfProducts() =>
        [
            new()
            {
                Id = Guid.Parse("11d837be-a763-488d-a52e-9ff21ac9d1c2"),
                Name = "IPhone X",
                Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                ImageFile = "product-1.png",
                Price = 950.00M,
                Category = ["Iphone"]
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
}
