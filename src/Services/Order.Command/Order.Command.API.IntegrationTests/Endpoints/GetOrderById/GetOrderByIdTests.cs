using System.Net;
using IntegrationTests.Common;
using Order.Command.API.Authorization;
using Order.Command.Domain.Models;
using Order.Command.API.Endpoints.GetOrderById;

namespace Order.Command.API.IntegrationTests.Endpoints.GetOrderById;

[Collection(GetWebApiContainerFactory.Name)]
public class GetOrderByIdTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
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
    public async Task GetOrderById_when_no_token_should_return_unauthorized()
    {
        // Arrange
        const string orderId = "invalid-order-id";
        string customerId = Guid.NewGuid().ToString();
        
        // Act
        var response = await _httpClient
            .GetAsync($"customers/{customerId}/orders/{orderId}",
                _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetOrderById_when_no_permission_should_return_forbidden()
    {
        // Arrange
        const string orderId = "invalid-order-id";
        string customerId = Guid.NewGuid().ToString();
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions([],
                sub: customerId))
            .GetAsync($"customers/{customerId}/orders/{orderId}",
                _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task GetOrderById_when_valid_token_but_user_tries_to_access_another_users_order_should_return_forbidden()
    {
        // Arrange
        const string orderId = "invalid-order-id";
        string customerId = Guid.NewGuid().ToString();
        string anotherCustomerId = Guid.NewGuid().ToString();
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanGetOrderPermission],
                sub: customerId))
            .GetAsync($"customers/{anotherCustomerId}/orders/{orderId}",
                _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task GetOrderById_when_invalid_order_id_should_return_bad_request()
    {
        // Arrange
        const string orderId = "invalid-order-id";
        string customerId = Guid.NewGuid().ToString();
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanGetOrderPermission],
                sub: customerId))
            .GetAsync($"customers/{customerId}/orders/{orderId}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problemDetails =
            await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldNotBeNull();
        problemDetails.Detail.ShouldContain("Invalid order id");
    }
    
    [Fact]
    public async Task GetOrderById_when_valid_order_id_should_return_ok()
    {
        // Arrange
        var orderId = Ulid.NewUlid();
        var customerId = Guid.NewGuid();
        await _sqlDbGiven.AnOrder(configure =>
        {
            configure.CustomerId = CustomerId.From(customerId);
            configure.Id = OrderId.From(orderId);
            configure.OrderItems =
            [
                new OrderItem(
                    OrderId.From(orderId),
                    ProductId.From(Ulid.NewUlid()),
                    1,
                    Price.From(10))
            ];
        });
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanGetOrderPermission],
                sub: customerId.ToString()))
            .GetAsync($"customers/{customerId}/orders/{orderId}",
            _cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        
        var result =
            await response.Content.ReadFromJsonAsync<Response>(cancellationToken: _cancellationToken);
        result.ShouldNotBeNull();
        
        var dbResult = _dbContext.Orders
            .Include(x => x.OrderItems)
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == OrderId.From(orderId));
        
        var expected = MapOrder(dbResult!);
        result.ShouldBeEquivalentTo(expected);
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }
    
    private static Response MapOrder(Domain.Models.Order order)
    {
        return new Response(
            Id: order.Id.Value.ToString(),
            CustomerId: order.CustomerId.Value.ToString(),
            OrderName: order.OrderName.Value,
            ShippingAddress: new ModuleAddress(
                order.ShippingAddress.FirstName,
                order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress,
                order.ShippingAddress.AddressLine,
                order.ShippingAddress.Country,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode
            ),
            BillingAddress:
            new ModuleAddress(
                order.BillingAddress.FirstName,
                order.BillingAddress.LastName,
                order.BillingAddress.EmailAddress,
                order.BillingAddress.AddressLine,
                order.BillingAddress.Country,
                order.BillingAddress.State,
                order.BillingAddress.ZipCode
            ),
            Payment: new ModulePayment(
                order.Payment.CardName,
                order.Payment.CardNumber,
                order.Payment.Expiration,
                order.Payment.CVV,
                order.Payment.PaymentMethod
            ),
            Status: order.Status.ToString(),
            OrderItems: order.OrderItems.Select(item => new ModuleOrderItem(
                item.ProductId.Value.ToString(),
                item.Quantity,
                item.Price.Value
            )).ToList()
        );
    }
}