using System.Net.Http.Headers;
using IntegrationTests.Common;
using Order.Command.API.Authorization;
using Order.Command.API.Endpoints.DeleteOrder;
using Order.Command.API.IntegrationTests.AutoFixture;

namespace Order.Command.API.IntegrationTests.Endpoints.DeleteOrder;

[Collection(GetWebApiContainerFactory.Name)]
public class DeleteOrderValidatorTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainerFactory);
        await _apiSpecification.InitializeAsync();
        _httpClient = _apiSpecification.HttpClient();
        _cancellationToken = _apiSpecification.CancellationToken;
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_no_token_should_return_unauthorized(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };
        
        // Act
        var response = await _httpClient
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_no_permission_should_return_forbidden(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_trying_to_delete_another_users_order_should_return_forbidden(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [],
                sub: Guid.NewGuid().ToString()))
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_invalid_etag_return_bad_request(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanDeleteOrderPermission],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("If-Match header is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task DeleteOrderValidator_when_invalid_order_id_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderId = "invalid-order-id"
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanDeleteOrderPermission],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain($"{request.OrderId} is not a valid Ulid");
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }
}