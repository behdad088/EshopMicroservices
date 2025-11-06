using Catalog.API.Features.Products.GetProductById;

namespace Catalog.API.IntegrationTests.Features.GetProductById
{
    [Collection(TestCollection.Name)]
    public class GetProductByIdTests(ApiSpecification apiSpecification) : IAsyncLifetime
    {
        private DataSeeder _dataSeeder = default!;
        private HttpClient _client = default!;

        public async Task InitializeAsync()
        {
            _dataSeeder = apiSpecification.DataSeeder;
            _client = apiSpecification.HttpClient;
            await apiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }

        [Fact]
        public async Task GetProductById_Product_NotFound_Returns_NotFound()
        {
            // Arrange
            var productId = Ulid.NewUlid();

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/{productId}");
            var response = await result.Content.ReadFromJsonAsync<string>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.ShouldNotBeNull();
            response.ShouldBe(productId.ToString());
        }

        [Fact]
        public async Task GetProductById_Returns_The_Product()
        {
            // Arrange
            var productId = Ulid.NewUlid().ToString();
            await _dataSeeder.SeedDataBaseAsync(x =>
            {
                x.Id = productId;
                x.Name = "IPhone X";
                x.Description =
                    "test description";
                x.ImageFile = "product-1.png";
                x.Price = 950.00M;
                x.Category = ["Smart Phone"];
            });

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/{productId}");
            var response = await result.Content.ReadFromJsonAsync<GetProductByIdResponse>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.ShouldNotBeNull();
            response.Id.ShouldBe(Ulid.Parse(productId));
            response.Name.ShouldBe("IPhone X");
            response.Description.ShouldBe("test description");
            response.ImageFile.ShouldBe("product-1.png");
            response.Price.ShouldBe(950.00M);
            var category = response.Category.ShouldHaveSingleItem();
            category.ShouldBe("Smart Phone");
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}