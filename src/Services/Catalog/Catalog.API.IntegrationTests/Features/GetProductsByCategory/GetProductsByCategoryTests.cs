using Catalog.API.Features.Products.GetProductByCategory;

namespace Catalog.API.IntegrationTests.Features.GetProductsByCategory
{
    [Collection(GetWebApiContainerFactory.Name)]
    public class GetProductsByCategoryTests(ApiSpecification ApiSpecification) : IClassFixture<ApiSpecification>, IAsyncLifetime
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
        public async Task GetProductsByCategory_Null_Category_Returns_BadRequest()
        {
            // Ararnge
            var category = "%20";

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/category/{category}");
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();
            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain($"Category cannot be null");
        }

        [Fact]
        public async Task GetProductsByCategory_Save_Two_Catalog_With_Differenct_Category_Returns_One()
        {
            // Ararnge
            await _dataSeeder.SeedDataBaseAsync();
            var product = DataSeeder.GetListOfProducts().FirstOrDefault(x => x.Name == "IPhone X");
            var category = product!.Category[0];

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products/category/{category}");
            var response = await result.Content.ReadFromJsonAsync<GetProductByCategoryResponse>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.ShouldNotBeNull();
            response.ProductModule.Count.ShouldBe(1);
            var expected = product.Adapt<ProductModule>();
            response.ProductModule.Data.ShouldBeEquivalentTo(new List<ProductModule> { expected });
        }

        public async Task DisposeAsync()
        {
            await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }
    }
}
