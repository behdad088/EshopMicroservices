using System.Text.Json.Serialization;

namespace Order.Command.Application.Dtos;

public record UpdateOrderDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("customer_id")]
    string? CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    AddressDto ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    AddressDto BillingAddress,
    [property: JsonPropertyName("payment")]
    PaymentDto Payment,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("etag")] string? Version,
    [property: JsonPropertyName("order_items")]
    List<OrderItems> OrderItems);