using Marten;
using MassTransit;
using MassTransit.Testing;
using Order.Query.Data.Events;
using Order.Query.Data.Views.OrderView;
using Order.Query.EventProcessor.Consumer;
using Order.Query.EventProcessor.IntegrationTests.AutoFixture;
using Order.Query.EventProcessor.IntegrationTests.Given.SqlGiven;
using Order.Query.EventProcessor.IntegrationTests.Masstransit;
using Shouldly;

namespace Order.Query.EventProcessor.IntegrationTests.Views.OrderViewTests;

[Collection(TestCollection.Name)]
public class OrderViewOrderCreatedEventTest : IAsyncLifetime
{
    private readonly ApiFactory _apiFactory;
    private readonly CancellationToken _cancellationToken;
    private readonly IDocumentStore _documentStore;
    private readonly ITestHarness  _testHarness;
    private readonly SqlGiven _sqlGiven;
    private readonly TestConsumeObserver  _consumeObserver;

    public OrderViewOrderCreatedEventTest(WebApiContainerFactory webApiContainerFactory)
    {
        _apiFactory = new ApiFactory(webApiContainerFactory);
        _cancellationToken = _apiFactory.CancellationToken;
        _documentStore = _apiFactory.GetDocumentStore;
        _testHarness = _apiFactory.TestHarness;
        _sqlGiven = new SqlGiven(_documentStore);
        _consumeObserver = _apiFactory.ConsumeObserver;
    }

    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_order_id_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidOrderId = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                Id = invalidOrderId
            }
        };
        
        // Act
        await _testHarness.Bus.Publish(orderCreatedEvent, context => context.MessageId = messageId,
            cancellationToken: _cancellationToken);
        
        var consumed = await _testHarness.GetConsumerHarness<EventConsumer<OrderView, OrderCreatedEvent>>()
            .Consumed
            .SelectAsync<CloudEvent<OrderCreatedEvent>>(x => x.Context.MessageId == messageId, _cancellationToken)
            .First();
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        consumed.ShouldNotBeNull();
        consumed.Exception.Message.ShouldContain("'Id' must not be empty.");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_null_order_name_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidOrderName = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderName = invalidOrderName
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("Name is required.");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_null_customer_id_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCustomerId = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                CustomerId = invalidCustomerId
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Customer Id' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_empty_order_items_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var emptyOrderItems = new List<OrderCreatedEvent.OrderItem>();
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = emptyOrderItems
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("OrderItems should not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_product_id_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidProductId = "invalid product id";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: invalidProductId,
                        Quantity: 1,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("Product Id is not a valid Ulid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_null_product_price_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? nullPrice = null;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: 1,
                        Price: nullPrice)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Price' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_product_price_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int invalidPrice = 0;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: 1,
                        Price: invalidPrice)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Price' must be greater than '0'");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_null_product_quantity_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? nullPrice = null;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: nullPrice,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Quantity' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_total_price_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        decimal totalPrice = 100;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
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
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain(
            "Validation Failed due to: Total price 100 does not match total price 200 from products");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_product_quantity_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? invalidPrice = 0;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                OrderItems = 
                [
                    new OrderCreatedEvent.OrderItem(
                        Id: Ulid.NewUlid().ToString(),
                        ProductId: Ulid.NewUlid().ToString(),
                        Quantity: invalidPrice,
                        Price: 100)
                ]
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Quantity' must be greater than '0'");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_firstname_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingFirstname = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    Firstname = missingFirstname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("firstname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_lastname_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingLastname = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    Lastname = missingLastname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("lastname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_email_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingEmailAddress = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    EmailAddress = missingEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_email_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidEmailAddress = "invalid.email.address";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    EmailAddress = invalidEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_address_line_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingAddressLine = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    AddressLine = missingAddressLine
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("address_line is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_country_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCountry = "invalid_country";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    Country = invalidCountry
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("country is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_state_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingState = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    State = missingState
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("state is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_zipcode_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingZipCode = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    ZipCode = missingZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Zip Code' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_zipcode_shipping_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidZipCode = "123456";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                ShippingAddress = orderCreatedEvent.Data.ShippingAddress with
                {
                    ZipCode = invalidZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("zip_code is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_firstname_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingFirstname = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    Firstname = missingFirstname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("firstname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_lastname_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingLastname = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    Lastname = missingLastname
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("lastname is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_email_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingEmailAddress = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    EmailAddress = missingEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_email_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidEmailAddress = "invalid.email.address";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    EmailAddress = invalidEmailAddress
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("email_address is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_address_line_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingAddressLine = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    AddressLine = missingAddressLine
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("address_line is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_country_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCountry = "invalid_country";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    Country = invalidCountry
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("country is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_state_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingState = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    State = missingState
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("state is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_zipcode_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingZipCode = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    ZipCode = missingZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Zip Code' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_zipcode_billing_address_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidZipCode = "123456";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                BillingAddress = orderCreatedEvent.Data.BillingAddress with
                {
                    ZipCode = invalidZipCode
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("zip_code is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_cvv_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingCvv = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    Cvv = missingCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Cvv' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_cvv_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCvv = "string.Empty";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    Cvv = invalidCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("CVV is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_length_cvv_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCvv = "1234";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    Cvv = invalidCvv
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("CVV length is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_card_name_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var missingCardName = string.Empty;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    CardName = missingCardName
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("card_name is required");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_card_number_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidCardNumber = "1234543643";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    CardNumber = invalidCardNumber
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("card_number is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_invalid_expiration_date_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var invalidExpirationDate = "6543645/234";
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    Expiration = invalidExpirationDate
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("payment expiration is not valid");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_missing_payment_method_should_send_message_to_dlq(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        int? missingPaymentMethod = null;
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod with
                {
                    PaymentMethod = missingPaymentMethod
                }
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderCreatedEvent>>>(_cancellationToken)
            .First();
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Payment Method' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderCreatedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_outdated_version_id_should_not_update(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var orderId = Ulid.NewUlid();
        orderCreatedEvent = orderCreatedEvent with
        {
            Data = orderCreatedEvent.Data with
            {
                Id = orderId.ToString(),
                Version = 1
            }
        };

        await _sqlGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = orderId.ToString();
            x.OrderCreatedEventVersion = 2;
        });
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        result.ShouldNotBeNull();
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderCreatedEvent.Data.Id);
        orderView.OrderCreatedEventVersion.ShouldBe(2);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderCreatedEvent_new_event_should_update(
        CloudEvent<OrderCreatedEvent> orderCreatedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderCreatedEvent, messageId, _cancellationToken);
        
        // Assert
        result.ShouldNotBeNull();
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderCreatedEvent.Data.Id);
        var eventStream = session.Query<EventStream>().First(x => x.ViewId == orderCreatedEvent.Data.Id);
        
        orderView.ShouldNotBeNull();
        eventStream.ShouldNotBeNull();
        
        orderView.Id.ShouldBe(orderCreatedEvent.Data.Id);
        orderView.CreatedAt.ShouldBe(orderCreatedEvent.Data.CreatedAt);
        orderView.CreatedBy.ShouldBe(orderCreatedEvent.Data.CreatedBy);
        orderView.LastModified.ShouldBe(orderCreatedEvent.Data.LastModified);
        orderView.CustomerId.ShouldBe(orderCreatedEvent.Data.CustomerId);
        orderView.OrderName.ShouldBe(orderCreatedEvent.Data.OrderName);

        orderView.OrderItems.ShouldBeEquivalentTo(orderCreatedEvent.Data.OrderItems
            .Select(x => new OrderView.OrderItem(x.Id, x.ProductId, x.Quantity!.Value, x.Price!.Value)).ToList());
        
        orderView.BillingAddress.ShouldBeEquivalentTo(
            new OrderView.Address
            {
                Firstname = orderCreatedEvent.Data.BillingAddress.Firstname,
                Lastname = orderCreatedEvent.Data.BillingAddress.Lastname,
                EmailAddress = orderCreatedEvent.Data.BillingAddress.EmailAddress,
                AddressLine = orderCreatedEvent.Data.BillingAddress.AddressLine,
                Country = orderCreatedEvent.Data.BillingAddress.Country,
                State = orderCreatedEvent.Data.BillingAddress.State,
                ZipCode = orderCreatedEvent.Data.BillingAddress.ZipCode
            });

        orderView.ShippingAddress.ShouldBeEquivalentTo(
            new OrderView.Address
            {
                Firstname = orderCreatedEvent.Data.ShippingAddress.Firstname,
                Lastname = orderCreatedEvent.Data.ShippingAddress.Lastname,
                EmailAddress = orderCreatedEvent.Data.ShippingAddress.EmailAddress,
                AddressLine = orderCreatedEvent.Data.ShippingAddress.AddressLine,
                Country = orderCreatedEvent.Data.ShippingAddress.Country,
                State = orderCreatedEvent.Data.ShippingAddress.State,
                ZipCode = orderCreatedEvent.Data.ShippingAddress.ZipCode
            });
        
        orderView.PaymentMethod.ShouldBeEquivalentTo(
            new OrderView.Payment
            {
                CardName = orderCreatedEvent.Data.PaymentMethod.CardName,
                CardNumber = orderCreatedEvent.Data.PaymentMethod.CardNumber,
                Expiration = orderCreatedEvent.Data.PaymentMethod.Expiration,
                Cvv = orderCreatedEvent.Data.PaymentMethod.Cvv,
                PaymentMethod = orderCreatedEvent.Data.PaymentMethod.PaymentMethod!.Value
            });
        orderView.OrderStatus.ShouldBe(orderCreatedEvent.Data.OrderStatus);
        orderView.TotalPrice.ShouldBe(orderCreatedEvent.Data.TotalPrice);
        orderView.OrderCreatedEventVersion.ShouldBe(orderCreatedEvent.Data.Version!.Value);
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

