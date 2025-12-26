using Order.Query.API.Features.GetOrdersByName;

namespace Order.Query.API.IntegrationTests.Features.GetOrdersByName;

[Collection(TestCollection.Name)]
public class GetOrdersByNameTests(ApiFactory apiFactory) : IAsyncLifetime
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
    public async Task GetOrdersByName_WhenInvalidOrderName_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { OrderName = string.Empty };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain("'Order Name' must not be empty.");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByName_WhenInvalidPageIndex_ShouldReturnBadRequest(Request request)
    {
        // Arrange
        request = request with { PageIndex = -1 };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission]))
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
    public async Task GetOrdersByName_WhenInvalidPageSize_ShouldReturnBadRequest(
        int pageSize,
        Request request)
    {
        // Arrange
        request = request with { PageSize = pageSize };
        
        // Act
        var (response, error) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission]))
            .GETAsync<Endpoint, Request, ProblemDetails>(request);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        error.Detail!.ShouldContain($"'Page Size' must be between 1 and 100. You entered {pageSize}.");
    }
    
    [Theory]
    [DomainDataAuto]
    public async Task GetOrdersByName_WhenNoToken_ShouldReturnUnauthorized(Request request)
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
    public async Task GetOrdersByName_WhenValidRequest_ShouldReturnTheList(Request request)
    {
        // Arrange
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = request.OrderName!;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = request.OrderName!;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = request.OrderName!;
        });
        
        // Act
        var (response, result) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission]))
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
    public async Task GetOrdersByName_WhenTwoPageRequest_ShouldReturnTheList()
    {
        // Arrange
        var orderName = "Test Name";
        var requestOne = new Request(OrderName: orderName, PageIndex: 0, PageSize: 3);
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = orderName;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = orderName;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = orderName;
        });
        
        var requestTwo = new Request(OrderName: orderName, PageIndex: 1, PageSize: 3);
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = orderName;
        });
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = Ulid.NewUlid().ToString();
            x.OrderName = orderName;
        });
        
        // Act
        var (responseOne, resultOne) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission], sub: orderName))
            .GETAsync<Endpoint, Request, Response>(requestOne);
        
        var (responseTwo, resultTwo) = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersQueryCanGetOrdersListsByOrderNamePermission], sub: orderName))
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