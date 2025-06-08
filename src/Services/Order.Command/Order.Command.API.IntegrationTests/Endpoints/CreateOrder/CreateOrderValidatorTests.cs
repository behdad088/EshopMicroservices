using Order.Command.API.IntegrationTests.AutoFixture;
using Order.Command.API.Endpoints.CreateOrder;

namespace Order.Command.API.IntegrationTests.Endpoints.CreateOrder;

[Collection(GetWebApiContainerFactory.Name)]
public class CreateOrderValidatorTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken = default!;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainerFactory);
        await _apiSpecification.InitializeAsync();
        _httpClient = _apiSpecification.HttpClient();
        _cancellationToken = _apiSpecification.CancellationToken;
    }

    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_order_id_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            Id = "invalid-order-id"
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain($"{request.Id} is not a valid Ulid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_order_name_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderName = null
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("Name is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_customer_id_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            CustomerId = "invalid-customer-id"
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain($"{request.CustomerId} is not a valid UUID");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_order_items_are_null_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = null
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_order_items_are_empty_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = []
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_firstname_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Firstname = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("firstname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_lastname_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Lastname = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("lastname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_email_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                EmailAddress = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_email_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                EmailAddress = "invalid-email"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_address_line_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                AddressLine = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("address_line is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_country_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                Country = "test-country"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("country is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_state_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                State = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("state is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_zip_code_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                ZipCode = "invalid-zip-code"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("zip_code is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_zip_code_in_billing_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            BillingAddress = request.BillingAddress! with
            {
                ZipCode = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Zip Code' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_firstname_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                Firstname = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("firstname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_lastname_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                Lastname = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("lastname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_email_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                EmailAddress = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_email_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                EmailAddress = "invalid-email"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_address_line_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                AddressLine = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("address_line is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_country_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                Country = "test-country"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("country is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_state_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                State = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("state is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_zip_code_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                ZipCode = "invalid-zip-code"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("zip_code is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_zip_code_in_shipping_address_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            ShippingAddress = request.ShippingAddress! with
            {
                ZipCode = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Zip Code' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_null_cvv_in_payment_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = null
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Cvv' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_cvv_in_payment_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = "invalid-cvv"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task CreateOrderValidator_when_invalid_length_cvv_in_payment_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            Payment = request.Payment! with
            {
                Cvv = "2345"
            }
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }

    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_order_item_is_null_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = null
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_order_item_is_empty_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = []
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_invalid_product_id_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new("invalid-product-id", 1, 1)
            ]
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("invalid-product-id is not a valid Ulid");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_null_price_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), 1, null)
            ]
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Price' must not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_invalid_price_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), 1, -1)
            ]
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Price' must be greater than '0'");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_null_quantity_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), null, 1)
            ]
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Quantity' must not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task CreateOrderValidator_when_invalid_quantity_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), -1, 1)
            ]
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("orders", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Quantity' must be greater than '0'");
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }
}