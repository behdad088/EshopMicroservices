namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public record GetOrderByCustomerResponse(
    Ulid Id,
    Guid CustomerId,
    string OrderName,
    GetOrderByCustomerResponse.AddressResponse ShippingAddress,
    GetOrderByCustomerResponse.AddressResponse BillingAddress,
    GetOrderByCustomerResponse.PaymentResponse Payment,
    string Status,
    List<GetOrderByCustomerResponse.OrderItemResponse> OrderItems)
{
    public record OrderItemResponse(
        string? ProductId,
        int? Quantity,
        decimal? Price);

    public record AddressResponse(
        string Firstname,
        string Lastname,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode);

    public record PaymentResponse(
        string CardName,
        string CardNumber,
        string Expiration,
        string Cvv,
        int PaymentMethod);
}

