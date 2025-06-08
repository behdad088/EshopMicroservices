namespace Order.Command.Application.Orders.Queries.GetOrders;

public record GetOrderResponse(
    Ulid Id,
    Guid CustomerId,
    string OrderName,
    GetOrderResponse.AddressResponse ShippingAddress,
    GetOrderResponse.AddressResponse BillingAddress,
    GetOrderResponse.PaymentResponse Payment,
    string Status,
    List<GetOrderResponse.OrderItemResponse> OrderItems
)
{
    public record OrderItemResponse(
        string? ProductId,
        int? Quantity,
        decimal? Price
    );

    public record AddressResponse(
        string Firstname,
        string Lastname,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode
    );

    public record PaymentResponse(
        string CardName,
        string CardNumber,
        string Expiration,
        string Cvv,
        int PaymentMethod
    );
}