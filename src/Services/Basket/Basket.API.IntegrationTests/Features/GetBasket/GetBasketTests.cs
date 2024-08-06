using System.Net.Http.Json;
using Basket.API.IntegrationTests.Database.Postgres;
using Basket.API.IntegrationTests.Database.Redis;
using Microsoft.AspNetCore.Mvc;
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
    public async Task GetBasket_Basket_NotFound_Returns_NotFound()
    {
        // Arrange
        var username = "test";
        
        // Act
        var result = await _client.GetAsync($"api/v1/basket/{username}");
        var response = await result.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        response.ShouldNotBeNull();
        response.Detail.ShouldNotBeNull();
        response.Detail.ShouldBe($"Entity \"basket\" ({username}) was not found.");
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.GetDocumentStore().Advanced.ResetAllData();
    }
}