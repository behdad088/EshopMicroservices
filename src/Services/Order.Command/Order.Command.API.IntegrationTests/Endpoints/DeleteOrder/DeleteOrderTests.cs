using System.Net.Http.Headers;
using Order.Command.API.Endpoints.DeleteOrder;

namespace Order.Command.API.IntegrationTests.Endpoints.DeleteOrder;

[Collection(TestCollection.Name)]
public class DeleteOrderTests : IClassFixture<ApiSpecification>
{
    private HttpClient _httpClient = default!;
    private CancellationToken _cancellationToken;
    private SqlDbGiven _sqlDbGiven = default!;
    private IApplicationDbContext _dbContext = default!;
    private ITestHarness _testHarness = default!;

    public DeleteOrderTests(ApiSpecification apiSpecification)
    {
        apiSpecification.ClearDatabaseAsync().GetAwaiter().GetResult();
        _httpClient = apiSpecification.HttpClient();
        _cancellationToken = apiSpecification.CancellationToken;
        _sqlDbGiven = apiSpecification.SqlDbGiven;
        _dbContext = apiSpecification.DbContext;
        _testHarness = apiSpecification.TestHarness;
    }
    
    [Theory, OrderRequestAutoData]
    public async Task DeleteOrder_when_order_id_not_found_should_return_not_found(
        Request request)
    {
        // Arrange
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, 
            $"api/v1/customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\"", isWeak: true));

        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanDeleteOrderPermission],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task DeleteOrder_when_invalid_order_etag_should_return_precondition_failed(
        Request request)
    {
        // Arrange
        var orderId = OrderId.From(Ulid.Parse(request.OrderId));
        
        await _sqlDbGiven.AnOrder(x => 
            x.Id = orderId);

        request = request with
        {
            OrderId = orderId.Value.ToString()
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, 
            $"api/v1/customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"2\"", isWeak: true));

        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanDeleteOrderPermission],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }
    
    [Theory, OrderRequestAutoData]
    public async Task DeleteOrder_when_valid_request_should_return_created(
        Request request)
    {
        // Arrange
        await _testHarness.Start();
        var orderId = OrderId.From(Ulid.Parse(request.OrderId));

        await _sqlDbGiven.AnOrder(x =>
        {
            x.Id = orderId;
        });
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, 
            $"api/v1/customers/{request.CustomerId}/orders/{request.OrderId}")
        {
            Content = JsonContent.Create(request)
        };

        requestMessage.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\"", isWeak: true));

        // Act
        var response = await _httpClient
            .SetFakeBearerToken(FakePermission.GetPermissions(
                [Policies.OrdersCommandCanDeleteOrderPermission],
                sub: request.CustomerId))
            .SendAsync(requestMessage, _cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        Outbox? outboxMessage = null;
        while (!_cancellationToken.IsCancellationRequested)
        {
            // check if the message is dispatched from the outbox
            outboxMessage = await _dbContext.Outboxes
                .Where(x => x.AggregateId == AggregateId.From(Ulid.Parse(request.OrderId)) 
                            && x.IsDispatched.Equals(IsDispatched.Yes))
                .FirstOrDefaultAsync(_cancellationToken);
            if (outboxMessage != null)
                break;
            
            await Task.Delay(100, _cancellationToken);
        }
        
        // check rmq!
        outboxMessage.ShouldNotBeNull();
        
        var isMessagePublished = await _testHarness.Published.Any<CloudEvent<OrderDeletedEvent>>(_cancellationToken);
        isMessagePublished.ShouldBeTrue();
        var message = _testHarness.Published.Select<CloudEvent<OrderDeletedEvent>>(_cancellationToken)
            .FirstOrDefault()
            ?.Context.Message.Data;
        message.ShouldNotBeNull();
        message.Version.ShouldBe(2);
        outboxMessage.NumberOfDispatchTry.Value.ShouldBe(1);
    }
}