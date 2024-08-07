using System.Net.Http.Json;
using Basket.API.Features.DeleteBasket;
using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.Models;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Basket.API.IntegrationTests.Features.DeleteBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class DeleteBasketTests(WebApiContainerFactory webApiContainer) : IAsyncLifetime
{
    private PostgresDataSeeder _postgresDataSeeder = default!;
    private RedisDataSeeder _redisDataSeeder = default!;
    private HttpClient _client = default!;
    private ApiSpecification _apiSpecification = default!;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainer);
        await _apiSpecification.InitializeAsync().ConfigureAwait(false);

        _postgresDataSeeder = _apiSpecification.PostgresDataSeeder;
        _redisDataSeeder = _apiSpecification.RedisDataSeeder;
        _client = _apiSpecification.HttpClient;

        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
    }
    
    [Fact]
    public async Task DeleteBasket_Empty_Username_Returns_BadRequest()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        var username = "%20";
        
        // Act
        var result = await _client.DeleteAsync($"api/v1/basket/{username}", timeout);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Username is required");
    }
    
    [Fact]
    public async Task DeleteBasket_Deletes_Basket_From_Cache_And_PostgresDb_Returns_Ok()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        var username = "test username 1";
        
        var shoppingCart = new ShoppingCart()
        {
            Items =
            [
                new ShoppingCartItem()
                {
                    Color = "test color",
                    Price = 10,
                    ProduceId = Guid.NewGuid(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = username
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);
        
        // Assert value saved successfully
        var basketInRedis = await _redisDataSeeder.GetShoppingCartAsync(username, timeout);
        basketInRedis.ShouldNotBeNull();
        
        var basketInPostgresDb = await _postgresDataSeeder.GetBasketAsync(username, timeout);
        basketInPostgresDb.ShouldNotBeNull();
        
        // Act
        var result = await _client.DeleteAsync($"api/v1/basket/{username}", timeout);
        var response = await result.Content.ReadFromJsonAsync<DeleteBasketResponse>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        
        basketInRedis = await _redisDataSeeder.GetShoppingCartAsync(username, timeout);
        basketInRedis.ShouldBeNull();
        
        basketInPostgresDb = await _postgresDataSeeder.GetBasketAsync(username, timeout);
        basketInPostgresDb.ShouldBeNull();
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
    }
}