using Catalog.API.Features.Products.DeleteProduct;
using Catalog.API.Features.Products.GetProductById;

namespace Catalog.API.IntegrationTests.Features.DeleteProduct;

[Collection(GetWebApiContainerFactory.Name)]
public class DeleteProductTests(ApiSpecification ApiSpecification) : IClassFixture<ApiSpecification>, IAsyncLifetime
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
    public async Task DeleteProductById_Invalid_Product_Id_Returns_BadRequest()
    {
        // Arrange
        var productId = "%20";

        // Act
        var result = await _client.DeleteAsync($"api/v1/catalog/products/{productId}");
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain($"Product Id is not a valid UUID");
    }

    [Fact]
    public async Task DeleteProductById_Add_Product_To_Db_Check_The_Existance_And_Delete_The_Product_By_Id()
    {
        // Arrange
        var product = new Models.Product
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Test",
            Category = ["test"],
            ImageFile = "test",
            Price = 1
        };

        await _dataSeeder.SeedDataBaseAsync(product);

        // Act - Assert
        var savedProduct = await _client.GetAsync($"api/v1/catalog/products/{product.Id}");
        savedProduct.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var savedProductResult = await GetHttpResultAsync<GetProductByIdResponse>(savedProduct.Content);
        savedProductResult.ShouldNotBeNull();


        var response = await _client.DeleteAsync($"api/v1/catalog/products/{savedProductResult.Id}");
        var result = await GetHttpResultAsync<DeleteProductResponse>(response.Content);
        result!.IsSuccess.ShouldBeTrue();

        var retryFetchProduct = await _client.GetAsync($"api/v1/catalog/products/{product.Id}");
        retryFetchProduct.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    private static async Task<T?> GetHttpResultAsync<T>(HttpContent httpContent)
    {
        var result = await httpContent.ReadFromJsonAsync<T>();
        return result;
    }

    public async Task DisposeAsync()
    {
        await ApiSpecification.GetDocumentStore().Advanced.ResetAllData();
    }
}
