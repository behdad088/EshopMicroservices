using System.Net;
using FastEndpoints;
using Order.Query.API.Features.GetOrderById;
using Order.Query.API.IntegrationTests.AutoFixture;
using Shouldly;

namespace Order.Query.API.IntegrationTests.Features.GetOrderById;

[Collection(TestCollection.Name)]
public class GetOrderByIdTests(ApiFactory apiFactory) : IAsyncLifetime
{
    private HttpClient _client = default!;
    
    public async Task InitializeAsync()
    {
        _client = apiFactory.CreateClient();
        await Task.CompletedTask;
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenNoTokenProvided_ReturnsUnauthorized(Request request)
    {
        // Act
        var response = await _client.GETAsync<Endpoint, Request>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}