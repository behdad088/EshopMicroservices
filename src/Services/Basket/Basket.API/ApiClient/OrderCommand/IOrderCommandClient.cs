using Refit;

namespace Basket.API.ApiClient.OrderCommand;

public interface IOrderCommandClient
{
    [Headers("Content-Type: application/json")]
    [Post("/api/v1/customers/{customerId}/orders/{orderId}")]
    Task<CreateOrderResponse> CreateOrder(
        [AliasAs("customerId")] string customerId,
        [AliasAs("orderId")] string orderId,
        [Body] CreateOrderRequest request,
        CancellationToken cancellationToken
    );
}