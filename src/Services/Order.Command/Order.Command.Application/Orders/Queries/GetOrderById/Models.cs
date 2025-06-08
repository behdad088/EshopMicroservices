namespace Order.Command.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdResponse(
    Ulid Id,
    Guid? CustomerId,
    string OrderName,
    GetOrderByIdResponse.AddressResponse ShippingAddress,
    GetOrderByIdResponse.AddressResponse BillingAddress,
    GetOrderByIdResponse.PaymentResponse Payment,
    string Status,
    int Version,
    List<GetOrderByIdResponse.OrderItemResponse> OrderItems
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
        int PaymentMethod);
}