namespace Order.Command.Application.Dtos;

public record OrderDto(
    Guid Id,
    Guid? CustomerId,
    string OrderName,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    PaymentDto Payment,
    string Status,
    List<OrderItemsDto> OrderItems);