using Catalog.API.Features.Products.GetProductById;

namespace Catalog.API.IntegrationTests.Features.GetProductById
{
    [Collection(GetWebApiContainerFactory.Name)]
    public class GetProductByIdTests(ApiSpecification ApiSpecification) : IClassFixture<ApiSpecification>, IAsyncLifetime
    {
        private DataSeeder _dataSeeder = default!;
        private HttpClient _client = default!;

        public async Task InitializeAsync()
        {
            _dataSeeder = ApiSpecification.DataSeeder;
            _client = ApiSpecification.HtpClient;
            await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }

        [Fact]
        public async Task GetProductById_Product_NotFound_Returns_NotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/{productId}");
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldBe($"Entity \"Product\" ({productId}) was not found.");
        }

        [Fact]
        public async Task GetProductById_Returns_The_Product()
        {
            // Arrange
            var product = new Product()
            {
                Id = Guid.Parse("a897cc49-02ea-436e-ae37-1129a991da35"),
                Name = "IPhone X",
                Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                ImageFile = "product-1.png",
                Price = 950.00M,
                Category = ["Smart Phone"]
            };
            await _dataSeeder!.SeedDataBaseAsync(product);

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/{product.Id}");
            var response = await result.Content.ReadFromJsonAsync<GetProductByIdResponse>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.ShouldNotBeNull();
            var expected = product.Adapt<GetProductByIdResponse>();
            response.ShouldBeEquivalentTo(expected);
        }

        public async Task DisposeAsync()
        {
            await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }
    }
}
