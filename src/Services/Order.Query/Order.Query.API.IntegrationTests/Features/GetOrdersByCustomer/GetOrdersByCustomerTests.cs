using Order.Query.API.Features.GetOrdersByCustomer;

namespace Order.Query.API.IntegrationTests.Features.GetOrdersByCustomer;

[Collection(TestCollection.Name)]
public class GetOrdersByCustomerTests(ApiFactory apiFactory) : IAsyncLifetime
{
    private HttpClient _client = default!;
    private DbGiven _dbGiven = null!;
    
    public async Task InitializeAsync()
    {
        _client = apiFactory.CreateClient();
        _dbGiven = apiFactory.DbGiven;
        await apiFactory.GetDocumentStore.Advanced.ResetAllData(CancellationToken.None);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenInvalidCustomerId_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { CustomerId = "invalid-customer-id" };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain("invalid-customer-id is not a valid UUID");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenInvalidPageIndex_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { PageIndex = -1 };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain("'Page Index' must be greater than or equal to '0'");
    }
    
    [Theory]
    [CustomInlineDomainDataAuto(-1)]
    [CustomInlineDomainDataAuto(0)]
    [CustomInlineDomainDataAuto(101)]
    public async Task GetOrdersByCustomer_WhenInvalidPageSize_ShouldReturnBadRequest(
        int pageSize,
        Request request)
    {
        // Arrange
        request = request with { PageSize = pageSize };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain($"'Page Size' must be between 1 and 100. You entered {pageSize}.");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenNoToken_ShouldReturnUnauthorized(Request request)
    {
        // Act
        var (response, _) = await _client
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenNoPolicy_ShouldReturnForbidden(Request request)
    {
        // Act
        var (response, _) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions([]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenCustomerId_In_Token_Is_Not_Valid_ShouldReturnForbidden(Request request)
    {
        // Arrange
        var invalidCustomerId = Guid.NewGuid().ToString();
        // Act
        var (response, _) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission], sub: invalidCustomerId))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByCustomer_WhenValidRequest_ShouldReturnTheList(Request request)
    {
        // Arrange
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = request.CustomerId!;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = request.CustomerId!;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = request.CustomerId!;
        });
        
        // Act
        var (response, result) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission], sub: request.CustomerId))
            .GETAsync<Endpoint, Request, Response>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.Orders.ShouldNotBeNull();
        result.Orders.Count.ShouldBe(3);
        result.Orders.Data!.Count().ShouldBe(3);
    }
    
    [Fact]
    public async Task GetOrdersByCustomer_WhenTwoPageRequest_ShouldReturnTheList()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var requestOne = new Request(CustomerId: customerId, PageIndex: 0, PageSize: 3);
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = customerId;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = customerId;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = customerId;
        });
        
        var requestTwo = new Request(CustomerId: customerId, PageIndex: 1, PageSize: 3);
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = customerId;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.CustomerId = customerId;
        });
        
        // Act
        var (responseOne, resultOne) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission], sub: customerId))
            .GETAsync<Endpoint, Request, Response>(requestOne);
        
        var (responseTwo, resultTwo) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByCustomerIdPermission], sub: customerId))
            .GETAsync<Endpoint, Request, Response>(requestTwo);
    
        // Assert
        responseOne.ShouldNotBeNull();
        responseOne.StatusCode.ShouldBe(HttpStatusCode.OK);
        resultOne.ShouldNotBeNull();
        resultOne.Orders.ShouldNotBeNull();
        resultOne.Orders.Count.ShouldBe(5);
        resultOne.Orders.Data!.Count().ShouldBe(3);
        
        responseTwo.ShouldNotBeNull();
        responseTwo.StatusCode.ShouldBe(HttpStatusCode.OK);
        resultTwo.ShouldNotBeNull();
        resultTwo.Orders.ShouldNotBeNull();
        resultTwo.Orders.Count.ShouldBe(5);
        resultTwo.Orders.Data!.Count().ShouldBe(2);
    }
    
    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}