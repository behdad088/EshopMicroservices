using IntegrationTests.Common;
using MassTransit.Testing;
using Order.Command.API.Authorization;
using Order.Command.API.IntegrationTests.AutoFixture;
using Order.Command.API.Endpoints.CreateOrder;
using Order.Command.Application.Rmq.CloudEvent.Models;
using Order.Command.Domain.Events;
using Order.Command.Domain.Models;

namespace Order.Command.API.IntegrationTests.Endpoints.CreateOrder;

[Collection(TestCollection.Name)]
public class CreateOrderTests : IClassFixture<ApiSpecification>
{
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken;
    private IApplicationDbContext _dbContext = default!;
    private ITestHarness _testHarness = default!;

    public CreateOrderTests(ApiSpecification apiSpecification)
    {
        apiSpecification.ClearDatabaseAsync().GetAwaiter().GetResult();
        _httpClient = apiSpecification.HttpClient();
        _cancellationToken = apiSpecification.CancellationToken;
        _dbContext = apiSpecification.DbContext;
        _testHarness = apiSpecification.TestHarness;
    }

    [Theory, OrderRequestAutoData]
    public async Task CreateOrder_when_valid_request_should_return_created(
        Request request)
    {
        // Arrange
        await _testHarness.Start();
        
        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanCreateOrderPermission],
                sub: request.CustomerId))
            .PostAsJsonAsync($"api/v1/customers/{request.CustomerId}/orders/{request.Id}", request, _cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Response>(_cancellationToken);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.ToString().ShouldContain($"customers/{request.CustomerId}/orders/{request.Id}");
        result.ShouldNotBeNull();
        result.Id.ShouldNotBeNull();
        
        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(Ulid.Parse(result!.Id)) 
                            && x.IsDispatched.Equals(IsDispatched.Yes))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // check rmq!
        outboxMessage.ShouldNotBeNull();
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderCreatedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderCreatedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.OrderName.ShouldBe(request.OrderName);
        message.CustomerId.ShouldBe(Guid.Parse(request.CustomerId!));
        message.Version.ShouldBe(1);
        message.TotalPrice.ShouldBe(request.OrderItems!.Sum(x => x.Price!.Value * x.Quantity!.Value));
        message.PaymentMethod.PaymentMethod.ShouldBe(request.Payment!.PaymentMethod!.Value);
        message.PaymentMethod.Cvv.ShouldBe(request.Payment!.Cvv);
        message.PaymentMethod.Expiration.ShouldBe(request.Payment!.Expiration);
        message.PaymentMethod.CardName.ShouldBe(request.Payment!.CardName);
        message.PaymentMethod.CardNumber.ShouldBe(request.Payment!.CardNumber);
        message.OrderItems.Count.ShouldBe(request.OrderItems!.Count);
        message.OrderItems.Select(x => 
                new Request.ModuleOrderItem(x.ProductId.ToString(), x.Quantity, x.Price)).ToList()
            .ShouldBeEquivalentTo(request.OrderItems);
        
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }
}