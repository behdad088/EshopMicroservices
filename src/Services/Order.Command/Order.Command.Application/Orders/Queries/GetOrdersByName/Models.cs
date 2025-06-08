namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public record GetOrderByNameResponse(
    Ulid Id,
    Guid CustomerId,
    string OrderName,
    GetOrderByNameResponse.AddressResponse ShippingAddress,
    GetOrderByNameResponse.AddressResponse BillingAddress,
    GetOrderByNameResponse.PaymentResponse Payment,
    string Status,
    List<GetOrderByNameResponse.OrderItemResponse> OrderItems
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