using Basket.API.Features.CheckoutBasket;
using Basket.API.IntegrationTests.ApiGivens;

namespace Basket.API.IntegrationTests.Features.CheckoutBasket;

[Collection(GetWebApiContainerFactory.Name)]
public class CheckoutBasketTests : BaseEndpoint
{
    private HttpClient _client = default!;
    private PostgresDataSeeder _postgresDataSeeder = default!;
    private RedisDataSeeder _redisDataSeeder = default!;
    private OrderCommandGiven _OrderCommandGiven = default!;
    
    public CheckoutBasketTests(ApiSpecification apiSpecification) : base(apiSpecification)
    {
        _postgresDataSeeder = _apiSpecification.PostgresDataSeeder;
        _redisDataSeeder = _apiSpecification.RedisDataSeeder;
        _client = _apiSpecification.HttpClient;
        _OrderCommandGiven = _apiSpecification.CreateOrderCommandServerGiven();
    }
    
    [Theory]
    [BasketRequestAutoData]
    public async Task CheckoutBasket_no_token_returns_unauthorized(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();

        // Act
        var result = await _client
            .PostAsJsonAsync("api/v1/basket/customers/checkout", request, timeout);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory]
    [BasketRequestAutoData]
    public async Task CheckoutBasket_no_permission_returns_forbidden(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();

        // Act
        var result = await _client
            .SetFakeBearerToken(request.Username)
            .PostAsJsonAsync("api/v1/basket/customers/checkout", request, timeout);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory]
    [BasketRequestAutoData]
    public async Task CheckoutBasket_invalid_order_name_returns_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        var orderName = string.Empty;
        request = request with { OrderName = orderName };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("order_name must be at least 5 characters long");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_firstname_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Firstname = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("firstname is required");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_lastname_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Lastname = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("lastname is required");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_email_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                EmailAddress = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is required");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_invalid_email_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                EmailAddress = "invalid-email"
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is not valid");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_address_line_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                AddressLine = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("address_line is required");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_invalid_country_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Country = "test-country"
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("country is not valid");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_state_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                State = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("state is required");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_invalid_zip_code_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                ZipCode = "invalid-zip-code"
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("zip_code is not valid");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_zip_code_in_billing_address_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                ZipCode = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Zip Code' must not be empty");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_null_cvv_in_payment_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = null
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Cvv' must not be empty");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_invalid_cvv_in_payment_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = "invalid-cvv"
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_invalid_length_cvv_in_payment_should_return_bad_request(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = "2345"
            }
        };
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_order_command_returns_un_authorized_should_return_un_authorized(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = request.Username!
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);

        _OrderCommandGiven.ReturningUnauthorized();
        
        // Act
        var response = await CallSutAsync(request, timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_order_command_returns_forbidden_should_return_forbidden(CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = request.Username!
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);

        _OrderCommandGiven.ReturningForbidden();
        
        // Act
        var response = await CallSutAsync(request, timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_order_command_returns_internal_server_error_should_return_internal_server_error(
        CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = request.Username!
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);

        _OrderCommandGiven.ReturningInternalServerError();
        
        // Act
        var response = await CallSutAsync(request, timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_order_command_returns_bad_request_should_return_bad_request(
        CheckoutBasketRequest request)
    {
        // Arrange
        var timeout = _apiSpecification.CreateTimeoutToken();
        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = request.Username!
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);

        _OrderCommandGiven.ReturningBadRequest(new ValidationErrors(
            [
                new ValidationError("order_id", "invalid order id")
            ]));
        
        // Act
        var response = await CallSutAsync(request, timeout);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        var error = result.Extensions.ShouldHaveSingleItem();
        error.Value?.ToString()?.ShouldContain("invalid order id");
    }
    
    
    [Theory, BasketRequestAutoData]
    public async Task CheckoutBasket_when_valid_request_should_return_ok_with_order_id(
        CheckoutBasketRequest request)
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var orderId = Ulid.NewUlid().ToString();
        var timeout = _apiSpecification.CreateTimeoutToken();
        var shoppingCart = new ShoppingCart
        {
            Items =
            [
                new ShoppingCartItem
                {
                    Color = "test color",
                    Price = 10,
                    ProductId = Ulid.NewUlid().ToString(),
                    ProductName = "test product name",
                    Quantity = 1
                }
            ],
            Username = request.Username!
        };

        await _postgresDataSeeder.SeedDatabaseAsync(shoppingCart, timeout);
        await _redisDataSeeder.AddShoppingCartAsync(shoppingCart, timeout);
        _OrderCommandGiven.ACreateOrderSuccessResponse(customerId, orderId);
        
        // Act
        var response = await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketCheckoutPermission],
                sub: customerId,
                username: request.Username))
            .PostAsJsonAsync("api/v1/basket/customers/checkout", request, timeout);
        var result = await response.Content.ReadFromJsonAsync<CheckoutBasketResponse>(timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result!.OrderId.ShouldBe(orderId);
    }
    
    private async Task<HttpResponseMessage> CallSutAsync(CheckoutBasketRequest request, CancellationToken timeout)
    {
        return await _client
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.BasketUserBasketCheckoutPermission],
                username: request.Username))
            .PostAsJsonAsync("api/v1/basket/customers/checkout", request, timeout);
    }
}