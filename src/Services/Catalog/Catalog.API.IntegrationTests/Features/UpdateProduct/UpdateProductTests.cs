﻿using System.Net;
using Catalog.API.Authorization;
using Catalog.API.Features.Products.UpdateProduct;

namespace Catalog.API.IntegrationTests.Features.UpdateProduct
{
    [Collection(GetWebApiContainerFactory.Name)]
    public class UpdateProductTests(WebApiContainerFactory webApiContainer) : IAsyncLifetime
    {
        private HttpClient _client = default!;
        private DataSeeder _dataSeeder = default!;
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
        public async Task UpdateProduct_No_Token_Return_Unauthorized(UpdateProductRequest updateProductRequest)
        {
            // Act
            var result = await _client
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    
        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_Invalid_Access_Policy_Return_Forbidden(UpdateProductRequest updateProductRequest)
        {
            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Null_Id_Returns_BadRequest(UpdateProductRequest updateProductRequest)
        {
            // Arrange
            updateProductRequest = updateProductRequest with { Id = default };

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("Product Id is required");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Invalid_Name_Length_Returns_BadRequest(
            UpdateProductRequest updateProductRequest)
        {
            // Arrange
            updateProductRequest = updateProductRequest with { Name = "a" };

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("must be between 2 and 150 characters");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Invalid_Category_Value_Returns_BadRequest(
            UpdateProductRequest updateProductRequest)
        {
            // Arrange
            updateProductRequest = updateProductRequest with { Category = [string.Empty] };

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("Category item cannot be null");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Null_ImageFile_Value_Returns_BadRequest(
            UpdateProductRequest updateProductRequest)
        {
            // Arrange
            updateProductRequest = updateProductRequest with { ImageFile = string.Empty };

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("ImageFile cannot be null");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Invalid_Price_Value_Returns_BadRequest(
            UpdateProductRequest updateProductRequest)
        {
            // Arrange
            updateProductRequest = updateProductRequest with { Price = 0 };

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain("Price must be greater than 0");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_ProductId_NotFound_Returns_NotFound(UpdateProductRequest updateProductRequest)
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("If-Match", "W/\"1\"");
            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);
            var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.ShouldNotBeNull();
            response.Detail.ShouldNotBeNull();
            response.Detail.ShouldContain($"Entity \"Product\" ({updateProductRequest.Id}) was not found.");
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Invalid_Etag_ReturnsStatus412PreconditionFailed(
            UpdateProductRequest updateProductRequest)
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("If-Match", "W/\"0\"");
            var product = new Product
            {
                Id = updateProductRequest.Id,
                Name = "Test",
                Description = "Test",
                Category = ["test"],
                ImageFile = "test",
                Price = 1
            };

            await _dataSeeder.SeedDataBaseAsync(product);

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        }

        [Theory, CatalogRequestAutoData]
        public async Task UpdateProduct_With_Valid_Object_Ok(UpdateProductRequest updateProductRequest)
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("If-Match", "W/\"1\"");
            var product = new Product
            {
                Id = updateProductRequest.Id,
                Name = "Test",
                Description = "Test",
                Category = ["test"],
                ImageFile = "test",
                Price = 1
            };

            await _dataSeeder.SeedDataBaseAsync(product);

            // Act
            var result = await _client
                .SetFakeBearerToken(FakePermission.GetPermissions([Policies.CatalogProductUpdatePermission]))
                .PutAsJsonAsync("api/v1/catalog/products", updateProductRequest);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            await using var session = _apiSpecification.GetDocumentStore().LightweightSession();
            var valueInDb = session.Query<Product>().FirstOrDefault(x => x.Id == updateProductRequest.Id);

            valueInDb!.Name.ShouldBe(updateProductRequest.Name);
            valueInDb.Description.ShouldBe(updateProductRequest.Description);
            valueInDb.Category.ShouldBe(updateProductRequest.Category);
            valueInDb.ImageFile.ShouldBe(updateProductRequest.ImageFile);
            valueInDb.Price.ShouldBe(updateProductRequest.Price);
            valueInDb.Version.ShouldBe(2);
        }

        public async Task DisposeAsync()
        {
            await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
            await _apiSpecification.DisposeAsync().ConfigureAwait(false);
        }
    }
}