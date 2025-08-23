using System.Net.Http.Headers;
using IntegrationTests.Common;
using Order.Command.API.Authorization;
using Order.Command.API.IntegrationTests.AutoFixture;
using Order.Command.API.Endpoints.UpdateOrder;

namespace Order.Command.API.IntegrationTests.Endpoints.UpdateOrder;

[Collection(TestCollection.Name)]
public class UpdateOrderValidatorTests : IClassFixture<ApiSpecification>
{
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken;

    public UpdateOrderValidatorTests(ApiSpecification apiSpecification)
    {
        apiSpecification.ClearDatabaseAsync().GetAwaiter().GetResult();
        _httpClient = apiSpecification.HttpClient();
        _cancellationToken = apiSpecification.CancellationToken;
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_no_token_should_return_unauthorized(Request request)
    {
        // Act
        var response = await _httpClient
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_no_permission_should_return_forbidden(Request request)
    {
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions([], sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_customer_updates_another_customers_order_should_return_forbidden(
        Request request)
    {
        // Arrange
        var anotherCustomer = Guid.NewGuid().ToString();
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{anotherCustomer}/orders/{request.Id}", request, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_order_id_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            Id = "invalid-order-id"
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain($"{request.Id} is not a valid Ulid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_order_name_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderName = null
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("Name is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_customer_id_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            CustomerId = "invalid-customer-id"
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain($"{request.CustomerId} is not a valid UUID");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_order_items_are_null_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = null
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_order_items_are_empty_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = []
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_firstname_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("firstname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_lastname_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("lastname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_email_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_email_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_address_line_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("address_line is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_country_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("country is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_state_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("state is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_zip_code_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("zip_code is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_zip_code_in_billing_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Zip Code' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_firstname_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("firstname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_lastname_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("lastname is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_email_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_email_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("email_address is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_address_line_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("address_line is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_country_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("country is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_state_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("state is required");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_zip_code_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("zip_code is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_zip_code_in_shipping_address_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Zip Code' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_null_cvv_in_payment_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Cvv' must not be empty");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_cvv_in_payment_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrderValidator_when_invalid_length_cvv_in_payment_should_return_bad_request(Request request)
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("CVV is not valid");
    }

    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_order_item_is_null_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = null
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, 
            $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_order_item_is_empty_should_return_bad_request(Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = []
        };
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request,
            _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("OrderItems should not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_invalid_product_id_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new("invalid-product-id", 1, 1)
            ]
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("invalid-product-id is not a valid Ulid");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_null_price_in_order_item_should_return_bad_request(
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Price' must not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_invalid_price_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), 1, -1)
            ]
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Price' must be greater than '0'");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_null_quantity_in_order_item_should_return_bad_request(
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
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .PutAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request,
            _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Quantity' must not be empty");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_invalid_quantity_in_order_item_should_return_bad_request(
        Request request)
    {
        // Arrange
        request = request with
        {
            OrderItems = [
                new(Ulid.NewUlid().ToString(), -1, 1)
            ]
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("'Quantity' must be greater than '0'");
    }
    
    [Theory, OrderRequestAutoData] 
    public async Task UpdateOrderValidator_when_invalid_etag_return_bad_request(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\""));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
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
    public async Task UpdateOrderValidator_when_invalid_status_return_bad_request(
        Request request)
    {
        // Arrange
        
        request = request with
        {
            Status = "invalid-status"
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/v1/customers/{request.CustomerId}/orders/{request.Id}")
        {
            Content = JsonContent.Create(request)
        };
        
        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\"", isWeak: true));
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(
                FakePermission.GetPermissions(
                    [Policies.OrdersCommandCanUpdateOrderPermission],
                    sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>(_cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ShouldNotBeNull();
        result.Detail.ShouldNotBeNull();
        result.Detail.ShouldContain("Invalid order status.");
    }
}