﻿using Catalog.API.Features.Products.CreateProduct;
using Catalog.API.IntegrationTests.AutoFixture;

namespace Catalog.API.IntegrationTests.Features.CreateProducts;

[Collection(GetWebApiContainerFactory.Name)]
public class CreateProductsTests(WebApiContainerFactory webApiContainer) : IAsyncLifetime
{
    private HttpClient _client = default!;
    private ApiSpecification _apiSpecification = default!;

    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainer);
        await _apiSpecification.InitializeAsync();

        _client = _apiSpecification.HttpClient;
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData();
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_Null_Name_Will_Return_BadRequest(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { Name = string.Empty };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Name is required");
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_Null_Category_Will_Return_BadRequest(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { Category = [] };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Category is required");
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_Null_ImageFile_Will_Return_BadRequest(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { ImageFile = string.Empty };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("ImageFile is required");
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_Null_Price_Will_Return_BadRequest(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { Price = null };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Price is required");
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_Zero_Price_Will_Return_BadRequest(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { Price = 0 };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Price must be greater than 0");
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_With_Valid_Object_Return_Created(CreateProductRequest createProductRequest)
    {
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);

        // Assert
        result.Headers.Location.ShouldNotBeNull();
        var locationHeader = result.Headers.Location.ToString();
        var productId = locationHeader.Split('/').Last();
        Should.NotThrow(() => Ulid.Parse(productId));


        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        await using var session = _apiSpecification.GetDocumentStore().LightweightSession();
        var valueInDb = session.Query<Product>().First(x => x.Id == createProductRequest.Id);
        valueInDb.ShouldNotBeNull();
        valueInDb.Id.ShouldBe(productId);
    }

    [Theory, CatalogRequestAutoData]
    public async Task CreateProduct_With_Invalid_Id_Return_NotFound(CreateProductRequest createProductRequest)
    {
        // Arrange
        createProductRequest = createProductRequest with { Id = default };

        // Act
        var result = await _client.PostAsJsonAsync("api/v1/catalog/products", createProductRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Id must be a valid Ulid");
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }
}