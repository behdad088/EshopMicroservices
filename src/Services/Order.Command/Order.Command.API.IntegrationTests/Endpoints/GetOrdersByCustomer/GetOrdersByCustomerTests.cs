using Order.Command.API.Endpoints.GetOrdersByCustomer;

namespace Order.Command.API.IntegrationTests.Endpoints.GetOrdersByCustomer;

[Collection(GetWebApiContainerFactory.Name)]
public class GetOrdersByCustomerTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
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
    public async Task GetOrdersByCustomer_when_invalid_customer_id_should_return_bad_request()
    {
        // Arrange
        const string customerId = "invalid-customer-id";
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNull();
        problemDetails.Detail.ShouldContain("Customer Id must be a valid GUID");
    }
    
    [Fact]
    public async Task GetOrdersByCustomer_when_invalid_page_index_should_return_bad_request()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        const int pageIndex = -1;
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId}&page_index={pageIndex}",
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
    public async Task GetOrdersByCustomer_when_invalid_page_size_should_return_empty_list()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        const int pageSize = -1;
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId}&page_size={pageSize}",
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
    public async Task GetOrdersByCustomer_when_empty_db_should_return_null()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId}",
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
    public async Task GetOrdersByCustomer_when_one_item_found_in_db_should_return_one_item()
    {
        // Arrange
        var customerId = CustomerId.From(Guid.NewGuid());
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.CustomerId = customerId;
        });
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToArray();
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 1, MapOrder(dbResult)));
        
        result.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task GetOrdersByCustomer_when_multiple_items_found_in_db_should_return_all_items()
    {
        // Arrange
        var customerId = CustomerId.From(Guid.NewGuid());
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.CustomerId = customerId;
        });
        
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.CustomerId = customerId;
        });
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToArray();
        
        var expected = new Response(new PaginatedItems<ModuleOrder>(0, 10, 2, MapOrder(dbResult)));        
        result.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task GetOrdersByCustomer_when_multiple_customers_in_db_should_return_correct_items()
    {
        // Arrange
        var customerId = CustomerId.From(Guid.NewGuid());
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.CustomerId = customerId;
        });
        
        await _sqlDbGiven.AnOrder(configuration =>
        {
            configuration.CustomerId = customerId;
        });
        
        await _sqlDbGiven.AnOrder();
        
        // Act
        var response = await _httpClient.GetAsync($"orders/customer?customer_id={customerId.Value}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ToArray();
        
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