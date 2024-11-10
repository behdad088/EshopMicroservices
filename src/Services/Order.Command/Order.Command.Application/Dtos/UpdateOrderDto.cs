using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record UpdateOrderDto(
    [property: JsonPropertyName("id")] Ulid Id,
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
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("order_items")]
    List<OrderItems> OrderItems);