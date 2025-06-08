using System.Net.Http.Headers;
using MassTransit.Testing;
using Order.Command.API.IntegrationTests.AutoFixture;
using Order.Command.API.Endpoints.UpdateOrder;
using Order.Command.Application.Rmq.CloudEvent.Models;
using Order.Command.Domain.Events;
using Order.Command.Domain.Models;

namespace Order.Command.API.IntegrationTests.Endpoints.UpdateOrder;

[Collection(GetWebApiContainerFactory.Name)]
public class UpdateOrderTests(WebApiContainerFactory webApiContainerFactory) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken;
    private SqlDbGiven _sqlDbGiven = default!;
    private IApplicationDbContext _dbContext = default!;
    private ITestHarness _testHarness = default!;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(webApiContainerFactory);
        await _apiSpecification.InitializeAsync();
        _httpClient = _apiSpecification.HttpClient();
        _cancellationToken = _apiSpecification.CancellationToken;
        _sqlDbGiven = _apiSpecification.SqlDbGiven;
        _dbContext = _apiSpecification.DbContext;
        _testHarness = _apiSpecification.TestHarness;
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrder_when_order_id_not_found_should_return_not_found(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, "orders")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\"", isWeak: true));

        // Act
        var response = await _httpClient.SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrder_when_invalid_order_etag_should_return_precondition_failed(
        Request request)
    {
        // Arrange
        var orderId = OrderId.From(Ulid.Parse(request.Id));
        await _sqlDbGiven.AnOrder(x => 
            x.Id = orderId);

        request = request with
        {
            Id = orderId.Value.ToString()
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, "orders")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\"", isWeak: true));

        // Act
        var response = await _httpClient.SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task UpdateOrder_when_valid_request_should_return_created(
        Request request)
    {
        // Arrange
        await _testHarness.Start();
        var orderId = OrderId.From(Ulid.Parse(request.Id));
        var customerId = CustomerId.From(Guid.Parse(request.CustomerId!));

        await _sqlDbGiven.AnOrder(x =>
        {
            x.Id = orderId;
            x.CustomerId = customerId;
        });

        request = request with
        {
            OrderName = "Updated Order Name",
            ShippingAddress = new ModuleAddress(
                "Test first name - updated",
                "Test last name - updated",
                "update@test.com",
                "Test street - updated",
                "Sweden",
                "city - update",
                "12345")
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, "orders")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));

        // Act
        var response = await _httpClient.SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(Ulid.Parse(request.Id)) 
                            && x.IsDispatched.Equals(IsDispatched.Yes))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // check rmq!
        outboxMessage.ShouldNotBeNull();
        
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderUpdatedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderUpdatedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.Version.ShouldBe(2);
        message.OrderName.ShouldBe(request.OrderName);
        message.CustomerId.ShouldBe(Guid.Parse(request.CustomerId!));
        message.TotalPrice.ShouldBe(request.OrderItems!.Sum(x => x.Price!.Value * x.Quantity!.Value));
        message.PaymentMethod.PaymentMethod.ShouldBe(request.Payment!.PaymentMethod!.Value);
        message.PaymentMethod.Cvv.ShouldBe(request.Payment!.Cvv);
        message.PaymentMethod.Expiration.ShouldBe(request.Payment!.Expiration);
        message.PaymentMethod.CardName.ShouldBe(request.Payment!.CardName);
        message.PaymentMethod.CardNumber.ShouldBe(request.Payment!.CardNumber);
        message.OrderItems.Count.ShouldBe(request.OrderItems!.Count);
        message.OrderItems.Select(x => 
                new ModuleOrderItem(x.ProductId.ToString(), x.Quantity, x.Price)).ToList()
            .ShouldBeEquivalentTo(request.OrderItems);
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync();
    }
}