using Order.Command.API.Endpoints.GetOrdersByName;

namespace Order.Command.API.IntegrationTests.Endpoints.GetOrdersByName;

[Collection(GetWebApiContainerFactory.Name)]
public class GetOrdersByNameTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken = default!;
    private SqlDbGiven _sqlDbGiven = default!;
    private IApplicationDbContext _dbContext = default!;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainerFactory);
        await _apiSpecification.InitializeAsync();
        _httpClient = _apiSpecification.HttpClient();
        _cancellationToken = _apiSpecification.CancellationToken;
        _sqlDbGiven = _apiSpecification.SqlDbGiven;
        _dbContext = _apiSpecification.DbContext;
    }
    
    [Fact]
    public async Task GetOrdersByName_when_invalid_order_name_should_return_bad_request()
    {
        // Arrange
        const string orderName = "%20";
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNull();
        problemDetails.Detail.ShouldContain("'Name' must not be empty");
    }

    [Fact]
    public async Task GetOrdersByName_when_invalid_page_index_should_return_bad_request()
    {
        // Arrange
        var orderName = "test-order-name";
        const int pageIndex = -1;
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName}&page_index={pageIndex}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNull();
        problemDetails.Detail.ShouldContain("'Page Index' must be greater than or equal to '0'");
    }
    
    [Fact]
    public async Task GetOrdersByName_when_invalid_page_size_should_return_empty_list()
    {
        // Arrange
        var orderName = "test-order-name";
        const int pageSize = -1;
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName}&page_size={pageSize}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNull();
        problemDetails.Detail.ShouldContain("'Page Size' must be between 1 and 100");
    }
    
    [Fact]
    public async Task GetOrdersByName_when_empty_db_should_return_null()
    {
        // Arrange
        var oredrName = "test-order-name";
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={oredrName}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 0, new List<ModuleOrder>()));
        result.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task GetOrdersByName_when_order_name_not_found_should_return_null()
    {
        // Arrange
        var orderName = "test-order-name";
        await _sqlDbGiven.AnOrder(configure =>
        {
            configure.OrderName = OrderName.From("another-test-order-name");
        });
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 0, new List<ModuleOrder>()));
        result.ShouldBeEquivalentTo(expected);
        
        _dbContext.Orders.FirstOrDefault(x => x.OrderName.Equals(OrderName.From("another-test-order-name"))).ShouldNotBeNull();
    }
    
    [Fact]
    public async Task GetOrdersByName_when_one_item_found_in_db_should_return_one_item()
    {
        // Arrange
        var orderName = OrderName.From("test-order-name");
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.OrderName = orderName;
        });
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.OrderName.Equals(orderName))
            .ToArray();
        
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 1, MapOrder(dbResult)));
        result.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task GetOrdersByName_when_multiple_items_found_in_db_should_return_all_items()
    {
        // Arrange
        var orderName = OrderName.From("test-order-name");
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.OrderName = orderName;
        });
        
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.OrderName = orderName;
        });
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.OrderName.Equals(orderName))
            .ToArray();
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 2, MapOrder(dbResult)));
        
        result.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task GetOrdersByName_when_multiple_customers_in_db_should_return_correct_items()
    {
        // Arrange
        var orderName = OrderName.From("test-order-name");
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.OrderName = orderName;
        });
        
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.OrderName = orderName;
        });
        
        await _sqlDbGiven.AnOrder();
        
        // Act
        var response = await _httpClient.GetAsync($"orders/name?name={orderName.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        result.Orders.Count.ShouldBe(2);
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.OrderName.Equals(orderName))
            .ToArray();
        
        var dbAllResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .ToArray();
        dbAllResult.Length.ShouldBe(3);
        
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 2, MapOrder(dbResult)));
        result.ShouldBeEquivalentTo(expected);
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }

    private static List<ModuleOrder> MapOrder(IEnumerable<Domain.Models.Order> orders)
    {
        return orders.Select(x => new ModuleOrder(
            Id: x.Id.Value.ToString(),
            CustomerId: x.CustomerId.Value.ToString(),
            OrderName: x.OrderName.Value,
            ShippingAddress: new ModuleAddress(
                x.ShippingAddress.FirstName,
                x.ShippingAddress.LastName,
                x.ShippingAddress.EmailAddress,
                x.ShippingAddress.AddressLine,
                x.ShippingAddress.Country,
                x.ShippingAddress.State,
                x.ShippingAddress.ZipCode
            ),
            BillingAddress:
            new ModuleAddress(
                x.BillingAddress.FirstName,
                x.BillingAddress.LastName,
                x.BillingAddress.EmailAddress,
                x.BillingAddress.AddressLine,
                x.BillingAddress.Country,
                x.BillingAddress.State,
                x.BillingAddress.ZipCode
            ),
            Payment: new ModulePayment(
                x.Payment.CardName,
                x.Payment.CardNumber,
                x.Payment.Expiration,
                x.Payment.CVV,
                x.Payment.PaymentMethod
            ),
            Status: x.Status.ToString(),
            OrderItems: x.OrderItems.Select(item => new ModuleOrderItem(
                item.ProductId.Value.ToString(),
                item.Quantity,
                item.Price.Value
            )).ToList()
        )).ToList();
    }
}