using System.Net;
using FastEndpoints;
using IntegrationTests.Common;
using Order.Query.Api.Authorization;
using Order.Query.API.Features.GetOrderById;
using Order.Query.API.IntegrationTests.AutoFixture;
using Order.Query.API.IntegrationTests.Given.DbGiven;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;
using Shouldly;

namespace Order.Query.API.IntegrationTests.Features.GetOrderById;

[Collection(TestCollection.Name)]
public class GetOrderByIdTests(ApiFactory apiFactory) : IAsyncLifetime
{
    private HttpClient _client = default!;
    private DbGiven _dbGiven;
    public async Task InitializeAsync()
    {
        _client = apiFactory.CreateClient();
        _dbGiven = apiFactory.DbGiven;
        
        await Task.CompletedTask;
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenInvalidOrderId_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { OrderId = "invalid-order-id" };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrderPermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain("invalid-order-id is not a valid Ulid");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenInvalidCustomerId_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { CustomerId = "invalid-customer-id" };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrderPermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain("invalid-customer-id is not a valid UUID");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenNoToken_ShouldReturnUnauthorized(Request request)
    {
        // Act
        var response = await _client
            .GETAsync<Endpoint, Request>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenNoPermission_ShouldReturnForbidden(Request request)
    {
        // Act
        var response = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [], sub: request.CustomerId))
            .GETAsync<Endpoint, Request>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenHavingPermissionButWrongUser_ShouldReturnForbidden(Request request)
    {
        // Act
        var response = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrderPermission]))
            .GETAsync<Endpoint, Request>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrderById_WhenExistingOrderId_ShouldReturnExpectedResponse(Request request)
    {
        // Arrange
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = request.OrderId!;
            x.CustomerId = request.CustomerId!;
        });
        
        // Act
        var (response, result) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrderPermission], sub: request.CustomerId))
            .GETAsync<Endpoint, Request, Response>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        result.ShouldNotBeNull();
        result.Id.ShouldBe(result.Id);
        result.CustomerId.ShouldBe(result.CustomerId);
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}