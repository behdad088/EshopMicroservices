using System.Net;
using Catalog.API.Authorization;
using Catalog.API.Features.Products.DeleteProduct;
using Catalog.API.Features.Products.GetProductById;

namespace Catalog.API.IntegrationTests.Features.DeleteProduct;

[Collection(GetWebApiContainerFactory.Name)]
public class DeleteProductTests(WebApiContainerFactory webApiContainer) : IAsyncLifetime
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

    [Theory, CatalogRequestAutoData]
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
    
    [Theory, CatalogRequestAutoData]
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
    public async Task DeleteProductById_Add_Product_To_Db_Check_The_Existance_And_Delete_The_Product_By_Id()
    {
        // Arrange
        var product = new Product
        {
            Id = Ulid.NewUlid().ToString(),
            Name = "Test",
            Description = "Test",
            Category = ["test"],
            ImageFile = "test",
            Price = 1
        };

        await _dataSeeder.SeedDataBaseAsync(product);

        // Act - Assert
        var savedProduct = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductDeletePermission]))
            .GetAsync($"api/v1/catalog/products/{product.Id}");
        savedProduct.StatusCode.ShouldBe(HttpStatusCode.OK);
        var savedProductResult = await GetHttpResultAsync<GetProductByIdResponse>(savedProduct.Content);
        savedProductResult.ShouldNotBeNull();


        var response = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductDeletePermission]))
            .DeleteAsync($"api/v1/catalog/products/{savedProductResult.Id}");
        var result = await GetHttpResultAsync<DeleteProductResponse>(response.Content);
        result!.IsSuccess.ShouldBeTrue();

        var retryFetchProduct = await _client.GetAsync($"api/v1/catalog/products/{product.Id}");
        retryFetchProduct.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<T?> GetHttpResultAsync<T>(HttpContent httpContent)
    {
        var result = await httpContent.ReadFromJsonAsync<T>();
        return result;
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }
}
