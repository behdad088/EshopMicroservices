using System.Net;
using System.Net.Http.Json;
using Basket.API.Authorization;
using Basket.API.Features.DeleteBasket;
using Basket.API.IntegrationTests.AutoFixture;
using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.Models;
using IntegrationTests.Common;
using Shouldly;

namespace Basket.API.IntegrationTests.Features.DeleteBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class DeleteBasketTests : BaseEndpoint
{
    private HttpClient _client = default!;
    private PostgresDataSeeder _postgresDataSeeder = default!;
    private RedisDataSeeder _redisDataSeeder = default!;

    public DeleteBasketTests(ApiSpecification apiSpecification) : base(apiSpecification)
    {
        _postgresDataSeeder = _apiSpecification.PostgresDataSeeder;
        _redisDataSeeder = _apiSpecification.RedisDataSeeder;
        _client = _apiSpecification.HttpClient;

    }

    [Theory]
    [BasketRequestAutoData]
    public async Task DeleteBasket_No_Token_Returns_Unauthorized(string username)
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;

        // Act
        var result = await _client
            .DeleteAsync($"api/v1/basket/{username}", timeout);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory]
    [BasketRequestAutoData]
    public async Task DeleteBasket_No_Permission_Returns_Forbidden(string username)
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;

        // Act
        var result = await _client
            .SetFakeBearerToken("sub")
            .DeleteAsync($"api/v1/basket/{username}", timeout);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteBasket_Deletes_Basket_From_Cache_And_PostgresDb_Returns_Ok()
    {
        // Arrange
        var timeout = _apiSpecification.CancellationToken;
        var username = "test username 1";

        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
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
        var result = await _client
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.BasketUserBasketDeletePermission],
                    username: username))
            .DeleteAsync($"api/v1/basket/{username}", timeout);
        var response = await result.Content.ReadFromJsonAsync<DeleteBasketResponse>(timeout);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();

        basketInRedis = await _redisDataSeeder.GetShoppingCartAsync(username, timeout);
        basketInRedis.ShouldBeNull();

        basketInPostgresDb = await _postgresDataSeeder.GetBasketAsync(username, timeout);
        basketInPostgresDb.ShouldBeNull();
    }
}