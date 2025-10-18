using System.Net;
using Shouldly;

namespace Order.Query.EventProcessor.IntegrationTests.Health;

[Collection(TestCollection.Name)]
public class ReadyTests(ApiFactory apiFactory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _httpClient = apiFactory.HttpClient;
    private readonly CancellationToken _cancellationToken = apiFactory.CancellationToken;
    
    [Fact]
    public async Task Health_Check_Should_ReturnsOk()
    {
        // Act
        var response = await _httpClient.GetAsync("/hc", _cancellationToken);
        
        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}