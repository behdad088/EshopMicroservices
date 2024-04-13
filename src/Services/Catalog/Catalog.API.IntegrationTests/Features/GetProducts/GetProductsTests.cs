using Catalog.API.Features.Products.GetProduct;

namespace Catalog.API.IntegrationTests.Features.GetProducts
{
    [Collection(GetWebApiContainerFactory.Name)]
    public class GetProductsTests(ApiSpecification ApiSpecification) : IClassFixture<ApiSpecification>, IAsyncLifetime
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
        public async Task GetList_With_Invalid_PageIndex_Will_Return_BadRequest()
        {
            // Arrange
            var pageIndex = -1;
            var pageSize = 10;

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products?page_size={pageSize}&page_index={pageIndex}");
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("Page index must be greater or equal to 0");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(21)]
        public async Task GetList_With_Invalid_PageSize_Will_Return_BadRequest(int pageSize)
        {
            // Arrange
            var pageIndex = 0;

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products?page_size={pageSize}&page_index={pageIndex}");
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("Page size must be between 2 to 20");
        }

        [Fact]
        public async Task GetList_With_Empty_Database_Should_Return_AnEmpty_List()
        {
            // Arrange
            var pageIndex = 0;
            var pageSize = 10;
            await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products?page_size={pageSize}&page_index={pageIndex}");
            var response = await result.Content.ReadFromJsonAsync<GetProductResponse>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.ShouldNotBeNull();
            response.Result.Data.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetList_With_With_Two_Items_In_Database_Should_Return_2_Items()
        {
            // Arrange
            var pageIndex = 0;
            var pageSize = 10;
            await _dataSeeder!.SeedDataBaseAsync();

            // Act
            var result = await _client.GetAsync($"api/v1/catalog/products?page_size={pageSize}&page_index={pageIndex}");
            var response = await result.Content.ReadFromJsonAsync<GetProductResponse>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.ShouldNotBeNull();
            response.Result.Data.ShouldNotBeEmpty();
            var expected = DataSeeder.GetListOfProducts().Adapt<List<ProductModule>>();
            response.Result.Data.ShouldBeEquivalentTo(expected);
            response.Result.Count.ShouldBeEquivalentTo((long)expected.Count);
        }

        public async Task DisposeAsync()
        {
            await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();
        }
    }
}
