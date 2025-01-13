namespace Order.Command.Application.Orders.Commands.CreateOrder;

public record OrderParameter(
    string? Id,
    string? CustomerId,
    string? OrderName,
    OrderParameter.Address? ShippingAddress,
    OrderParameter.Address? BillingAddress,
    OrderParameter.Payment? OrderPayment,
    List<OrderParameter.OrderItem>? OrderItems)
{
    public record Address(
        string Firstname,
        string Lastname,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode);

    public record Payment(
        string CardName,
        string CardNumber,
        string Expiration,
        string Cvv,
        int PaymentMethod);

    public record OrderItem(
        string Id,
        string? OrderId,
        string? ProductId,
        int? Quantity,
        decimal? Price);
}