using Marten;
using MassTransit;
using MassTransit.Testing;
using Order.Query.EventProcessor.IntegrationTests.AutoFixture;
using Order.Query.EventProcessor.IntegrationTests.Given.DbGiven;
using Order.Query.EventProcessor.IntegrationTests.Masstransit;
using Order.Query.Events;
using Order.Query.Features.OrderView;
using Shouldly;

namespace Order.Query.EventProcessor.IntegrationTests.Views.OrderViewTests;

[Collection(TestCollection.Name)]
public class OrderViewOrderUpdatedEventTest : IAsyncLifetime
{
    private readonly ApiFactory _apiFactory;
    private readonly CancellationToken _cancellationToken;
    private readonly IDocumentStore _documentStore;
    private readonly ITestHarness  _testHarness;
    private readonly DbGiven _dbGiven;
    private readonly TestConsumeObserver  _consumeObserver;

    public OrderViewOrderUpdatedEventTest(WebApiContainerFactory webApiContainerFactory)
    {
        _apiFactory = new ApiFactory(webApiContainerFactory);
        _cancellationToken = _apiFactory.CancellationToken;
        _documentStore = _apiFactory.GetDocumentStore;
        _testHarness = _apiFactory.TestHarness;
        _dbGiven = new DbGiven(_documentStore);
        _consumeObserver = _apiFactory.ConsumeObserver;
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_order_id_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdateEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidOrderId = string.Empty;
        orderUpdateEvent = orderUpdateEvent with
        {
            Data = orderUpdateEvent.Data with
            {
                Id = invalidOrderId
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdateEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Id' must not be empty.");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdateEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_null_order_name_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidOrderName = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderName = invalidOrderName
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("Name is required.");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_empty_order_items_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var emptyOrderItems = new List<OrderUpdatedEvent.OrderItem>();
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = emptyOrderItems
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("OrderItems should not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_product_id_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidProductId = "invalid product id";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: invalidProductId,
                        Quantity: 1,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("Product Id is not a valid Ulid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_null_product_price_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? nullPrice = null;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: 1,
                        Price: nullPrice)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Price' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task? OrderViewOrderUpdatedEvent_invalid_product_price_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int invalidPrice = 0;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: 1,
                        Price: invalidPrice)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Price' must be greater than '0'");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_null_product_quantity_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? nullPrice = null;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: nullPrice,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Quantity' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_total_price_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        decimal totalPrice = 100;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: 2,
                        Price: 100)
                ],
                TotalPrice = totalPrice
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain(
            "Validation Failed due to: Total price 100 does not match total price 200 from products");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_product_quantity_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? invalidPrice = 0;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderUpdatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: invalidPrice,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Quantity' must be greater than '0'");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_firstname_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingFirstname = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    Firstname = missingFirstname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("firstname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_lastname_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingLastname = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    Lastname = missingLastname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("lastname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_email_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingEmailAddress = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    EmailAddress = missingEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_email_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidEmailAddress = "invalid.email.address";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    EmailAddress = invalidEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_address_line_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingAddressLine = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    AddressLine = missingAddressLine
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("address_line is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_country_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCountry = "invalid_country";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    Country = invalidCountry
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("country is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_state_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingState = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    State = missingState
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("state is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_zipcode_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingZipCode = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    ZipCode = missingZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Zip Code' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_zipcode_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidZipCode = "123456";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                ShippingAddress = orderUpdatedEvent.Data.ShippingAddress with
                {
                    ZipCode = invalidZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("zip_code is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_firstname_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingFirstname = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    Firstname = missingFirstname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("firstname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_lastname_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingLastname = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    Lastname = missingLastname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("lastname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_email_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingEmailAddress = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    EmailAddress = missingEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_email_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidEmailAddress = "invalid.email.address";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    EmailAddress = invalidEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_address_line_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingAddressLine = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    AddressLine = missingAddressLine
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("address_line is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_country_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCountry = "invalid_country";
        orderUdatedEvent = orderUdatedEvent with
        {
            Data = orderUdatedEvent.Data with
            {
                BillingAddress = orderUdatedEvent.Data.BillingAddress with
                {
                    Country = invalidCountry
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("country is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_state_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingState = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    State = missingState
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("state is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_zipcode_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingZipCode = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    ZipCode = missingZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Zip Code' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_zipcode_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidZipCode = "123456";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                BillingAddress = orderUpdatedEvent.Data.BillingAddress with
                {
                    ZipCode = invalidZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("zip_code is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_cvv_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingCvv = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    Cvv = missingCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Cvv' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_cvv_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCvv = "string.Empty";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    Cvv = invalidCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("CVV is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_length_cvv_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCvv = "1234";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    Cvv = invalidCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("CVV is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_card_name_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingCardName = string.Empty;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    CardName = missingCardName
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("card_name is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_card_number_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCardNumber = "1234543643";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    CardNumber = invalidCardNumber
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("card_number is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_invalid_expiration_date_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidExpirationDate = "6543645/234";
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    Expiration = invalidExpirationDate
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("payment expiration is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_missing_payment_method_should_send_message_to_dlq(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? missingPaymentMethod = null;
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod with
                {
                    PaymentMethod = missingPaymentMethod
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderUpdatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Payment Method' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderUpdatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_outdated_version_id_should_not_update(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var orderId = Ulid.NewUlid();
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                Id = orderId.ToString(),
                Version = 1
            }
        };

        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = orderId.ToString();
            x.OrderCreatedEventVersion = 1;
            x.OrderUpdatedEventVersion = 2;
        });
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        result.ShouldNotBeNull();
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderUpdatedEvent.Data.Id);
        orderView.OrderCreatedEventVersion.ShouldBe(1);
        orderView.OrderUpdatedEventVersion.ShouldBe(2);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderUpdatedEvent_new_event_should_update(
        CloudEvent<OrderUpdatedEvent> orderUpdatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var orderId = Ulid.NewUlid();
        orderUpdatedEvent = orderUpdatedEvent with
        {
            Data = orderUpdatedEvent.Data with
            {
                Id = orderId.ToString(),
                Version = 3
            }
        };
        
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = orderId.ToString();
            x.OrderCreatedEventVersion = 1;
            x.OrderUpdatedEventVersion = 2;
        });
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderUpdatedEvent, messageId, _cancellationToken);
        
        // Assert
        result.ShouldNotBeNull();
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderId.ToString());
        var eventStream = session.Query<EventStream>().First(x => x.ViewId == orderId.ToString());
        
        orderView.ShouldNotBeNull();
        eventStream.ShouldNotBeNull();
        
        orderView.Id.ShouldBe(orderUpdatedEvent.Data.Id);
        orderView.LastModified.ShouldBe(orderUpdatedEvent.Data.LastModified);
        orderView.OrderName.ShouldBe(orderUpdatedEvent.Data.OrderName);

        orderView.OrderItems.ShouldBeEquivalentTo(orderUpdatedEvent.Data.OrderItems
            .Select(x => new OrderView.OrderItem(x.Id, x.ProductId, x.Quantity!.Value, x.Price!.Value)).ToList());
        
        orderView.BillingAddress.ShouldBeEquivalentTo(
            new OrderView.Address
            {
                Firstname = orderUpdatedEvent.Data.BillingAddress.Firstname,
                Lastname = orderUpdatedEvent.Data.BillingAddress.Lastname,
                EmailAddress = orderUpdatedEvent.Data.BillingAddress.EmailAddress,
                AddressLine = orderUpdatedEvent.Data.BillingAddress.AddressLine,
                Country = orderUpdatedEvent.Data.BillingAddress.Country,
                State = orderUpdatedEvent.Data.BillingAddress.State,
                ZipCode = orderUpdatedEvent.Data.BillingAddress.ZipCode
            });

        orderView.ShippingAddress.ShouldBeEquivalentTo(
            new OrderView.Address
            {
                Firstname = orderUpdatedEvent.Data.ShippingAddress.Firstname,
                Lastname = orderUpdatedEvent.Data.ShippingAddress.Lastname,
                EmailAddress = orderUpdatedEvent.Data.ShippingAddress.EmailAddress,
                AddressLine = orderUpdatedEvent.Data.ShippingAddress.AddressLine,
                Country = orderUpdatedEvent.Data.ShippingAddress.Country,
                State = orderUpdatedEvent.Data.ShippingAddress.State,
                ZipCode = orderUpdatedEvent.Data.ShippingAddress.ZipCode
            });
        
        orderView.PaymentMethod.ShouldBeEquivalentTo(
            new OrderView.Payment
            {
                CardName = orderUpdatedEvent.Data.PaymentMethod.CardName,
                CardNumber = orderUpdatedEvent.Data.PaymentMethod.CardNumber,
                Expiration = orderUpdatedEvent.Data.PaymentMethod.Expiration,
                Cvv = orderUpdatedEvent.Data.PaymentMethod.Cvv,
                PaymentMethod = orderUpdatedEvent.Data.PaymentMethod.PaymentMethod!.Value
            });
        orderView.OrderStatus.ShouldBe(orderUpdatedEvent.Data.OrderStatus);
        orderView.TotalPrice.ShouldBe(orderUpdatedEvent.Data.TotalPrice);
        orderView.OrderCreatedEventVersion.ShouldBe(1);
        orderView.OrderUpdatedEventVersion.ShouldBe(orderUpdatedEvent.Data.Version!.Value);
    }
    
    public async Task InitializeAsync()
    {
        await _testHarness.Start();
    }

    public async Task DisposeAsync()
    {
        await _testHarness.Stop(_cancellationToken);
    }
}