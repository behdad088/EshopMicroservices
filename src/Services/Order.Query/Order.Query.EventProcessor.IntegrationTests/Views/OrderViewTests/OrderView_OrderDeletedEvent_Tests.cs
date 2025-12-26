using System.Text.Json;
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
public class OrderViewOrderDeletedEventTests : IAsyncLifetime
{
    private readonly CancellationToken _cancellationToken;
    private readonly IDocumentStore _documentStore;
    private readonly ITestHarness  _testHarness;
    private readonly DbGiven _dbGiven;
    private readonly TestConsumeObserver _consumeObserver;
    
    public OrderViewOrderDeletedEventTests(WebApiContainerFactory webApiContainerFactory)
    {
        var apiFactory = new ApiFactory(webApiContainerFactory);
        _cancellationToken = apiFactory.CancellationToken;
        _documentStore = apiFactory.GetDocumentStore;
        _testHarness = apiFactory.TestHarness;
        _dbGiven = new DbGiven(_documentStore);
        _consumeObserver = apiFactory.ConsumeObserver;
    }

    [Theory, EventAutoData]
    public async Task OrderViewOrderDeletedEvent_invalid_order_id_should_send_message_to_dlq(
        CloudEvent<OrderDeletedEvent>  orderDeletedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        orderDeletedEvent = orderDeletedEvent with
        {
            Data = orderDeletedEvent.Data! with
            {
                OrderId = string.Empty
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderDeletedEvent, messageId, _cancellationToken);

        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderDeletedEvent>>>(_cancellationToken)
            .First();
        
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Order Id' must not be empty");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderDeletedEvent.Id);
    }
    
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderDeletedEvent_invalid_deleted_date_should_send_message_to_dlq(
        CloudEvent<OrderDeletedEvent>  orderDeletedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        orderDeletedEvent = orderDeletedEvent with
        {
            Data = orderDeletedEvent.Data! with
            {
                DeletedDate = "invalid date"
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderDeletedEvent, messageId, _cancellationToken);

        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderDeletedEvent>>>(_cancellationToken)
            .First();
        
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("invalid date is not a valid timestamp");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderDeletedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderDeletedEvent_invalid_version_should_send_message_to_dlq(
        CloudEvent<OrderDeletedEvent>  orderDeletedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        orderDeletedEvent = orderDeletedEvent with
        {
            Data = orderDeletedEvent.Data! with
            {
                Version = 0
            }
        };
        
        // Act
        var (_, exception) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderDeletedEvent, messageId, _cancellationToken);

        // Assert
        var fault = await _testHarness.Published
            .SelectAsync<Fault<CloudEvent<OrderDeletedEvent>>>(_cancellationToken)
            .First();
        
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("'Version' must be greater than or equal to '1'");
        fault.Context.Message.FaultedMessageId.ShouldBe(messageId);
        fault.Context.Message.Message.Id.ShouldBe(orderDeletedEvent.Id);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderDeletedEvent_outdated_version_should_not_update(
        CloudEvent<OrderDeletedEvent>  orderDeletedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var orderId = Ulid.NewUlid();
        orderDeletedEvent = orderDeletedEvent with
        {
            Data = orderDeletedEvent.Data! with
            {
                OrderId = orderId.ToString(),
                Version = 1
            }
        };
        
        await _dbGiven.OrderViewConfigurationGiven(x =>
        {
            x.Id = orderId.ToString();
            x.OrderDeletedEventVersion = 2;
        });
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderDeletedEvent, messageId, _cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderDeletedEvent.Data.OrderId);
        orderView.OrderDeletedEventVersion.ShouldBe(2);
    }
    
    [Theory, EventAutoData]
    public async Task OrderViewOrderDeletedEvent_should_process_the_event(
        CloudEvent<OrderDeletedEvent>  orderDeletedEvent)
    {
        // Arrange
        var messageId = Guid.NewGuid();
        
        // Act
        var (result, _) =
            await _consumeObserver.PublishEventAndWaitForConsumed(orderDeletedEvent, messageId, _cancellationToken);

        // Assert
        await using var session = _documentStore.LightweightSession();
        var orderView = session.Query<OrderView>().First(x => x.Id == orderDeletedEvent.Data!.OrderId);
        var eventStream = session.Query<EventStream>().First(x => x.ViewId == orderDeletedEvent.Data!.OrderId);
        
        orderView.ShouldNotBeNull();
        result.ShouldNotBeNull();
        orderView.Id.ShouldBe(orderDeletedEvent.Data!.OrderId);
        orderView.DeletedDate.ShouldBe(orderDeletedEvent.Data.DeletedDate);
        orderView.OrderDeletedEventVersion.ShouldBe(orderDeletedEvent.Data.Version!.Value);
        
        var eventStreamData = JsonSerializer.Deserialize<OrderDeletedEvent>(eventStream.Data);
        eventStreamData.ShouldNotBeNull();
        eventStreamData.OrderId.ShouldBe(orderDeletedEvent.Data.OrderId);
        eventStreamData.DeletedDate.ShouldBe(orderDeletedEvent.Data.DeletedDate);
        eventStreamData.Version.ShouldBe(orderDeletedEvent.Data.Version);
        eventStreamData.CreatedAt.ShouldBe(orderDeletedEvent.Data.CreatedAt);
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