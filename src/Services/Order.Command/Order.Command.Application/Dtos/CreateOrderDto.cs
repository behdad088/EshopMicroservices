using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record CreateOrderDto(
    [property: JsonPropertyName("id")]
    Guid Id,
    [property: JsonPropertyName("customer_id")]
    Guid? CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    AddressDto ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    AddressDto BillingAddress,
    [property: JsonPropertyName("payment")]
    PaymentDto Payment,
    [property: JsonPropertyName("order_items")]
    List<OrderItemsDto> OrderItems);