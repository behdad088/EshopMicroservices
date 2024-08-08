using System.Net.Http.Json;
using Basket.API.Features.StoreBasket;
using Basket.API.IntegrationTests.AutoFixture;
using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Basket.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shouldly;

namespace Basket.API.IntegrationTests.Features.StoreBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class StoreBasketTests(WebApiContainerFactory webApiContainer)  : IAsyncLifetime
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

    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Null_Username_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CancellationToken;
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(Username: string.Empty, request.ShoppingCart!.Items));
            
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Username is required.");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Null_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CancellationToken;
        request = new StoreBasketRequest(ShoppingCart: null);
            
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", request, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Cart cannot be null");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Null_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(Username: request.ShoppingCart!.Username, Items: null));
            
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", invalidRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Shopping items cannot be null");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Zero_Quantity_In_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CancellationToken;
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(
                Username: request.ShoppingCart!.Username,
                Items: [new BasketItem(0, "Test Color", 100, Guid.NewGuid(), "Test name")]));
            
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Quantity must be greater than 0.");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Zero_Price_In_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CancellationToken;
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(
                Username: request.ShoppingCart!.Username,
                Items: [new BasketItem(1, "Test Color", 0, Guid.NewGuid(), "Test name")]));
            
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Price must be greater than 0.");
    }

    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Valid_Request_Saves_Data_In_PostgresDb_And_Redis(StoreBasketRequest request)
    {
        // Arrange 
        var token = _apiSpecification.CancellationToken;
        
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", request, token);
        var response = await result.Content.ReadFromJsonAsync<StoreBasketResponse>(token);
        
        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.ShouldNotBeNull();

        var resultInPostgresDb =
            await _postgresDataSeeder.GetBasketAsync(username: request.ShoppingCart!.Username, token);
        resultInPostgresDb.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInPostgresDb!.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInPostgresDb.Items));

        var resultInRedis = await _redisDataSeeder.GetShoppingCartAsync(request.ShoppingCart.Username, token);
        resultInRedis.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInRedis.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInRedis.Items));
    }
    
    [Theory, BasketRequestAutoData]
    public async Task StoreBasket_Valid_Request_Saves_Data_With_Valid_TotalPrice(StoreBasketRequest request)
    {
        // Arrange 
        var token = _apiSpecification.CancellationToken;
        var validRequest = new StoreBasketRequest(new BasketDtoRequest(request.ShoppingCart!.Username,
            new List<BasketItem>()
            {
                request.ShoppingCart.Items!.FirstOrDefault()!,
                request.ShoppingCart.Items!.FirstOrDefault()!,
            }));
        
        // Act
        var result = await _client.PostAsJsonAsync("api/v1/basket", validRequest, token);
        var response = await result.Content.ReadFromJsonAsync<StoreBasketResponse>(token);
        
        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.ShouldNotBeNull();

        var resultInPostgresDb =
            await _postgresDataSeeder.GetBasketAsync(username: validRequest.ShoppingCart!.Username, token);
        resultInPostgresDb.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInPostgresDb!.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInPostgresDb.Items));
        resultInPostgresDb.TotalPrice.ShouldBe(200);

        var resultInRedis = await _redisDataSeeder.GetShoppingCartAsync(validRequest.ShoppingCart.Username, token);
        resultInRedis.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInRedis.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInRedis.Items));
        resultInRedis.TotalPrice.ShouldBe(200);
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData().ConfigureAwait(false);
    }
}