namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderParameter(
    string? Id,
    string? CustomerId,
    string? OrderName,
    UpdateOrderParameter.Address? ShippingAddress,
    UpdateOrderParameter.Address? BillingAddress,
    UpdateOrderParameter.Payment? OrderPayment,
    string? Status,
    string? Version,
    List<UpdateOrderParameter.OrderItem>? OrderItems)
{
    public record Address(
        string? Firstname,
        string? Lastname,
        string? EmailAddress,
        string? AddressLine,
        string? Country,
        string? State,
        string? ZipCode);

    public record Payment(
        string? CardName,
        string? CardNumber,
        string? Expiration,
        string? Cvv,
        int? PaymentMethod);

    public record OrderItem(
        string? ProductId,
        int? Quantity,
        decimal? Price);
}