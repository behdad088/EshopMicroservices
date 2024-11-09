using Catalog.API.Features.Products.GetProductById;

namespace Catalog.API.IntegrationTests.Features.GetProductById
{
    [Collection(GetWebApiContainerFactory.Name)]
    public class GetProductByIdTests(WebApiContainerFactory webApiContainer) : IAsyncLifetime
    {
        private DataSeeder _dataSeeder = default!;
        private HttpClient _client = default!;
        private ApiSpecification _apiSpecification = default!;

        public async Task InitializeAsync()
        {
            _apiSpecification = new ApiSpecification(webApiContainer);
            await _apiSpecification.InitializeAsync();

            _dataSeeder = _apiSpecification.DataSeeder;
            _client = _apiSpecification.HttpClient;
            await _apiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }

        [Fact]
        public async Task GetProductById_Product_NotFound_Returns_NotFound()
        {
            // Arrange
            var productId = Ulid.NewUlid();

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
                Id = Ulid.NewUlid().ToString(),
                Name = "IPhone X",
                Description =
                    "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
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
            var expected = MapToProduct(product);
            response.ShouldBeEquivalentTo(expected);
        }

        public async Task DisposeAsync()
        {
            await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
            await _apiSpecification.DisposeAsync().ConfigureAwait(false);
        }

        private static GetProductByIdResponse MapToProduct(Product product)
        {
            return new GetProductByIdResponse(
                Id: Ulid.Parse(product.Id),
                Name: product.Name,
                Category: product.Category,
                Description: product.Description,
                ImageFile: product.ImageFile,
                Price: product.Price);
        }
    }
}