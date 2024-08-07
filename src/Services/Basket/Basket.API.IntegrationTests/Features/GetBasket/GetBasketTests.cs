using System.Net.Http.Json;
using Basket.API.Features.GetBasket;
using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shouldly;

namespace Basket.API.IntegrationTests.Features.GetBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class GetBasketTests(WebApiContainerFactory webApiContainer): IAsyncLifetime
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
    public async Task GetBasket_Empty_Username_Returns_BadRequest()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        var username = "%20";
        
        // Act
        var result = await _client.GetAsync($"api/v1/basket/{username}", timeout);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain($"Username cannot be null");
    }

    [Fact]
    public async Task GetBasket_Basket_NotFound_Returns_NotFound()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        const string username = "test username 1";
        
        // Act
        var result = await _client.GetAsync($"api/v1/basket/{username}", timeout);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldBe($"Entity \"basket\" ({username}) was not found.");
    }
    
    [Fact]
    public async Task GetBasket_Basket_Only_Exists_In_Postgres_database_Should_Return_Basket_And_Add_To_Redis()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        const string username = "test username 2";
        
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
        
        //Assert Value in Redis db
        var basketInRedis = await _redisDataSeeder.GetShoppingCartAsync(username, timeout);
        basketInRedis.ShouldBeNull();
        
        
        // Act
        var result = await _client.GetAsync($"api/v1/basket/{username}", cancellationToken: timeout);
        var response = await result.Content.ReadFromJsonAsync<GetBasketResponse>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.Username.ShouldBe(shoppingCart.Username);
        response.TotalPrice.ShouldBe(shoppingCart.TotalPrice);
        JsonConvert.SerializeObject(response.Items).ShouldBe(JsonConvert.SerializeObject(shoppingCart.Items));
        basketInRedis = await _redisDataSeeder.GetShoppingCartAsync(username, timeout);
        basketInRedis.ShouldNotBeNull();
        basketInRedis.ShouldBeEquivalentTo(shoppingCart);
    }

    [Fact]
    public async Task GetBasket_Basket_Only_Exists_In_Redis_database_Should_Return_Basket()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        const string username = "test username 3";
        
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

        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);
        
        //Assert Value in postgres db
        var basketInPostgresDb = await _postgresDataSeeder.GetBasketAsync(username, timeout);
        basketInPostgresDb.ShouldBeNull();
        
        
        // Act
        var result = await _client.GetAsync($"api/v1/basket/{username}", cancellationToken: timeout);
        var response = await result.Content.ReadFromJsonAsync<GetBasketResponse>(cancellationToken: timeout);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.Username.ShouldBe(shoppingCart.Username);
        response.TotalPrice.ShouldBe(shoppingCart.TotalPrice);
        JsonConvert.SerializeObject(response.Items).ShouldBe(JsonConvert.SerializeObject(shoppingCart.Items));
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
    }
}