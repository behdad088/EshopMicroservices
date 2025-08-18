using MassTransit.Testing;
using Order.Command.API.Endpoints.CreateOrder;
using Order.Command.Application.Rmq.CloudEvent.Models;
using Order.Command.Domain.Events;
using Order.Command.Domain.Models;

namespace Order.Command.API.IntegrationTests.OutboxTests;

[Collection(GetWebApiContainerFactory.Name)]
public class OutboxServiceTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private CancellationToken _cancellationToken;
    private IApplicationDbContext _dbContext = default!;
    private SqlDbGiven _sqlDbGiven = default!;
    private ITestHarness _testHarness = default!;

    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainerFactory);
        await _apiSpecification.InitializeAsync();
        _cancellationToken = _apiSpecification.CancellationToken;
        _dbContext = _apiSpecification.DbContext;
        _testHarness = _apiSpecification.TestHarness;
        _sqlDbGiven = _apiSpecification.SqlDbGiven;
    }

    [Fact]
    public async Task? OutboxService_when_valid_order_created_event_is_not_dispatched_should_dispatch_the_event()
    {
        // Arrange
        var orderCreatedEvent = new OrderConfiguration().ToOrderDb();
        await _testHarness.Start();
        await _sqlDbGiven.AnOutbox(x =>
        {
            x.AggregateId = AggregateId.From(orderCreatedEvent.Id.Value);
            x.EventType = EventType.From(nameof(OrderCreatedEvent));
            x.Payload = Payload.Serialize(orderCreatedEvent.ToOrderCreatedEvent());
            x.DispatchDateTime = DispatchDateTime.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        });
        
        // Act
        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(orderCreatedEvent.Id.Value) 
                            && x.IsDispatched.Equals(IsDispatched.Yes) 
                            && x.EventType == EventType.From(nameof(OrderCreatedEvent)))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // Assert
        outboxMessage.ShouldNotBeNull();
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderCreatedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderCreatedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.OrderName.ShouldBe(orderCreatedEvent.OrderName.Value);
        message.CustomerId.ShouldBe(orderCreatedEvent.CustomerId.Value);
        message.Version.ShouldBe(1);
        message.TotalPrice.ShouldBe(orderCreatedEvent.OrderItems.Sum(x => x.Price.Value * x.Quantity));
        message.PaymentMethod.PaymentMethod.ShouldBe(orderCreatedEvent.Payment.PaymentMethod);
        message.PaymentMethod.Cvv.ShouldBe(orderCreatedEvent.Payment.CVV);
        message.PaymentMethod.Expiration.ShouldBe(orderCreatedEvent.Payment.Expiration);
        message.PaymentMethod.CardName.ShouldBe(orderCreatedEvent.Payment.CardName);
        message.PaymentMethod.CardNumber.ShouldBe(orderCreatedEvent.Payment.CardNumber);
        message.OrderItems.Count.ShouldBe(orderCreatedEvent.OrderItems.Count);
        var messageOrderItem = message.OrderItems.First();
        messageOrderItem.OrderId.ShouldBe(orderCreatedEvent.Id.Value);
        messageOrderItem.ProductId.ShouldBe(orderCreatedEvent.OrderItems[0].ProductId.Value);
        messageOrderItem.Quantity.ShouldBe(orderCreatedEvent.OrderItems[0].Quantity);
        messageOrderItem.Price.ShouldBe(orderCreatedEvent.OrderItems[0].Price.Value);
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }
    
    [Fact]
    public async Task? OutboxService_when_valid_order_updated_event_is_not_dispatched_should_dispatch_the_event()
    {
        // Arrange
        var orderUpdatedEvent = new OrderConfiguration().ToOrderDb();
        await _testHarness.Start();
        await _sqlDbGiven.AnOutbox(x =>
        {
            x.AggregateId = AggregateId.From(orderUpdatedEvent.Id.Value);
            x.EventType = EventType.From(nameof(OrderUpdatedEvent));
            x.Payload = Payload.Serialize(orderUpdatedEvent.ToOrderUpdatedEvent());
            x.DispatchDateTime = DispatchDateTime.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        });
        
        // Act

        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(orderUpdatedEvent.Id.Value) 
                            && x.IsDispatched.Equals(IsDispatched.Yes)
                            && x.EventType == EventType.From(nameof(OrderUpdatedEvent)))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // Assert
        outboxMessage.ShouldNotBeNull();
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderUpdatedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderUpdatedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.OrderName.ShouldBe(orderUpdatedEvent.OrderName.Value);
        message.CustomerId.ShouldBe(orderUpdatedEvent.CustomerId.Value);
        message.Version.ShouldBe(1);
        message.TotalPrice.ShouldBe(orderUpdatedEvent.OrderItems.Sum(x => x.Price.Value * x.Quantity));
        message.PaymentMethod.PaymentMethod.ShouldBe(orderUpdatedEvent.Payment.PaymentMethod);
        message.PaymentMethod.Cvv.ShouldBe(orderUpdatedEvent.Payment.CVV);
        message.PaymentMethod.Expiration.ShouldBe(orderUpdatedEvent.Payment.Expiration);
        message.PaymentMethod.CardName.ShouldBe(orderUpdatedEvent.Payment.CardName);
        message.PaymentMethod.CardNumber.ShouldBe(orderUpdatedEvent.Payment.CardNumber);
        message.OrderItems.Count.ShouldBe(orderUpdatedEvent.OrderItems.Count);
        var messageOrderItem = message.OrderItems.First();
        messageOrderItem.OrderId.ShouldBe(orderUpdatedEvent.Id.Value);
        messageOrderItem.ProductId.ShouldBe(orderUpdatedEvent.OrderItems[0].ProductId.Value);
        messageOrderItem.Quantity.ShouldBe(orderUpdatedEvent.OrderItems[0].Quantity);
        messageOrderItem.Price.ShouldBe(orderUpdatedEvent.OrderItems[0].Price.Value);
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }
    
    [Fact]
    public async Task? OutboxService_when_valid_order_deleted_event_is_not_dispatched_should_dispatch_the_event()
    {
        // Arrange
        var orderId = Ulid.NewUlid();
        var orderUpdatedEvent = new OrderDeletedEvent(orderId, DateTimeOffset.UtcNow.ToString(), 1);
        
        await _testHarness.Start();
        await _sqlDbGiven.AnOutbox(x =>
        {
            x.AggregateId = AggregateId.From(orderId);
            x.EventType = EventType.From(nameof(OrderDeletedEvent));
            x.Payload = Payload.Serialize(orderUpdatedEvent);
            x.DispatchDateTime = DispatchDateTime.From(DateTimeOffset.UtcNow.AddMinutes(-1));
        });
        
        // Act

        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(orderId) 
                            && x.IsDispatched.Equals(IsDispatched.Yes) 
                            && x.EventType == EventType.From(nameof(OrderDeletedEvent)))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // Assert
        outboxMessage.ShouldNotBeNull();
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderDeletedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderDeletedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.Version.ShouldBe(1);
        message.OrderId.ShouldBe(orderId);
        message.DeletedDate.ShouldBe(orderUpdatedEvent.DeletedDate);
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }
}