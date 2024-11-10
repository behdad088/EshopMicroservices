namespace Order.Command.Application.Orders.Commands.CreateOrder;

public record OrderDto(
    string Id,
    string? CustomerId,
    string OrderName,
    OrderDto.Address ShippingAddress,
    OrderDto.Address BillingAddress,
    OrderDto.Payment OrderPayment,
    List<OrderDto.OrderItem> OrderItems)
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





