using System.Net;

namespace Order.Command.API.IntegrationTests.HealthChecks;

[Collection(GetWebApiContainerFactory.Name)]
public class ReadyTests(ApiSpecification apiSpecification) : IClassFixture<ApiSpecification>
{
    private readonly HttpClient _httpClient = apiSpecification.HttpClient;
    private readonly CancellationToken _cancellationToken = apiSpecification.CancellationToken;
    
    [Fact]
    public async Task Health_Check_Should_ReturnsOk()
    {
        // Act
        var response = await _httpClient.GetAsync("hc", _cancellationToken);
        
        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}