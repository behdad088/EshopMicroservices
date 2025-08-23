using Catalog.API.Features.Products.DeleteProduct;
using Catalog.API.Features.Products.GetProductById;
using IntegrationTests.Common;

namespace Catalog.API.IntegrationTests.Features.DeleteProduct;

[Collection(TestCollection.Name)]
public class DeleteProductTests(ApiSpecification apiSpecification) : IAsyncLifetime
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
    public async Task DeleteProductById_No_Token_Return_Unauthorized()
    {
        // Arrange
        const string productId = "%20";

        // Act
        var result = await _client
            .DeleteAsync($"api/v1/catalog/products/{productId}");

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task DeleteProductById_Invalid_Access_Policy_Return_Forbidden()
    {
        // Arrange
        const string productId = "%20";

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([]))
            .DeleteAsync($"api/v1/catalog/products/{productId}");

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    
    [Fact]
    public async Task DeleteProductById_Invalid_Product_Id_Returns_BadRequest()
    {
        // Arrange
        const string productId = "%20";

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductDeletePermission]))
            .DeleteAsync($"api/v1/catalog/products/{productId}");
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain($"Product Id is not a valid UUID");
    }

    [Fact]
    public async Task DeleteProductById_Add_Product_To_Db_Check_The_Existence_And_Delete_The_Product_By_Id()
    {
        // Arrange
        var productId = Ulid.NewUlid().ToString();

        await _dataSeeder.SeedDataBaseAsync(x =>
        {
            x.Id = productId;
            x.Name = "Test";
            x.Description = "Test";
            x.Category = ["test"];
            x.ImageFile = "test";
            x.Price = 1;
        });

        // Act - Assert
        var savedProduct = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductDeletePermission]))
            .GetAsync($"api/v1/catalog/products/{productId}");
        savedProduct.StatusCode.ShouldBe(HttpStatusCode.OK);
        var savedProductResult = await GetHttpResultAsync<GetProductByIdResponse>(savedProduct.Content);
        savedProductResult.ShouldNotBeNull();


        var response = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductDeletePermission]))
            .DeleteAsync($"api/v1/catalog/products/{savedProductResult.Id}");
        var result = await GetHttpResultAsync<DeleteProductResponse>(response.Content);
        result!.IsSuccess.ShouldBeTrue();

        var retryFetchProduct = await _client.GetAsync($"api/v1/catalog/products/{productId}");
        retryFetchProduct.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<T?> GetHttpResultAsync<T>(HttpContent httpContent)
    {
        var result = await httpContent.ReadFromJsonAsync<T>();
        return result;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
