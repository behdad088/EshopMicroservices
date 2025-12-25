using Basket.API.Features.StoreBasket;
using Basket.API.IntegrationTests.ApiGivens;
using Basket.API.Models.Dtos;
using Newtonsoft.Json;

namespace Basket.API.IntegrationTests.Features.StoreBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class StoreBasketTests : BaseEndpoint
{
    private HttpClient _client;
    private DiscountGiven _discountGiven;
    private PostgresDataSeeder _postgresDataSeeder;
    private RedisDataSeeder _redisDataSeeder;

    public StoreBasketTests(ApiSpecification apiSpecification) : base(apiSpecification)
    {
        _postgresDataSeeder = _apiSpecification.PostgresDataSeeder;
        _redisDataSeeder = _apiSpecification.RedisDataSeeder;
        _client = _apiSpecification.HttpClient;
        _discountGiven = _apiSpecification.CreateDiscountServerGiven();
    }
    
    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Null_Username_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CreateTimeoutToken();
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(string.Empty, request.ShoppingCart!.Items));

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart.Username,
                roles: ["admin"]))
            .PostAsJsonAsync("api/v1/basket/customers", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Username is required.");
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Null_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CreateTimeoutToken();
        request = new StoreBasketRequest(null);

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username,
                roles: ["admin"]))
            .PostAsJsonAsync("api/v1/basket/customers", request, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Cart cannot be null");
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Null_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(request.ShoppingCart!.Username, null));

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username))
            .PostAsJsonAsync("api/v1/basket/customers", invalidRequest);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Shopping items cannot be null");
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Zero_Quantity_In_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CreateTimeoutToken();
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(
                request.ShoppingCart!.Username,
                [new BasketItem(0, "Test Color", 100, Ulid.NewUlid().ToString(), "Test name")]));

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username))
            .PostAsJsonAsync("api/v1/basket/customers", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Quantity must be greater than 0.");
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Zero_Price_In_Items_In_Request_Returns_BadRequest(StoreBasketRequest request)
    {
        // Arrange
        var token = _apiSpecification.CreateTimeoutToken();
        var invalidRequest =
            new StoreBasketRequest(new BasketDtoRequest(
                request.ShoppingCart!.Username,
                [new BasketItem(1, "Test Color", 0, Ulid.NewUlid().ToString(), "Test name")]));

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username))
            .PostAsJsonAsync("api/v1/basket/customers", invalidRequest, token);
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldContain("Price must be greater than 0.");
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Valid_Request_Saves_Data_In_PostgresDb_And_Redis(StoreBasketRequest request)
    {
        // Arrange 
        var token = _apiSpecification.CreateTimeoutToken();
        _discountGiven.GetDiscountGiven(request.ShoppingCart!.Items!.FirstOrDefault()!.ProductName);

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username))
            .PostAsJsonAsync("api/v1/basket/customers", request, token);
        var response = await result.Content.ReadFromJsonAsync<StoreBasketResponse>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();

        var resultInPostgresDb =
            await _postgresDataSeeder.GetBasketAsync(request.ShoppingCart!.Username, token);
        resultInPostgresDb.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInPostgresDb.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInPostgresDb.Items));

        var resultInRedis = await _redisDataSeeder.GetShoppingCartAsync(request.ShoppingCart.Username, token);
        resultInRedis.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInRedis.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInRedis.Items));
    }

    [Theory]
    [BasketRequestAutoData]
    public async Task StoreBasket_Valid_Request_Saves_Data_With_Valid_TotalPrice(StoreBasketRequest request)
    {
        // Arrange 
        var token = _apiSpecification.CreateTimeoutToken();
        var validRequest = new StoreBasketRequest(new BasketDtoRequest(request.ShoppingCart!.Username,
            new List<BasketItem>
            {
                request.ShoppingCart.Items!.FirstOrDefault()!,
                request.ShoppingCart.Items!.FirstOrDefault()!
            }));

        _discountGiven.GetDiscountGiven(request.ShoppingCart.Items!.FirstOrDefault()!.ProductName);

        // Act
        var result = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketStorePermission], 
                username: request.ShoppingCart?.Username))
            .PostAsJsonAsync("api/v1/basket/customers", validRequest, token);
        var response = await result.Content.ReadFromJsonAsync<StoreBasketResponse>(token);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();

        var resultInPostgresDb =
            await _postgresDataSeeder.GetBasketAsync(validRequest.ShoppingCart!.Username, token);
        resultInPostgresDb.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInPostgresDb.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInPostgresDb.Items));
        resultInPostgresDb.TotalPrice.ShouldBe(180);

        var resultInRedis = await _redisDataSeeder.GetShoppingCartAsync(validRequest.ShoppingCart.Username, token);
        resultInRedis.ShouldNotBeNull();
        response.ShoppingCart.Username.ShouldBe(resultInRedis.Username);
        JsonConvert.SerializeObject(response.ShoppingCart.Items)
            .ShouldBe(JsonConvert.SerializeObject(resultInRedis.Items));
        resultInRedis.TotalPrice.ShouldBe(180);
    }
}