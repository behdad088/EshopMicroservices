using System.Text.Json.Serialization;

namespace Order.Command.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdDto(
    [property: JsonPropertyName("id")] 
    Ulid Id,
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
    [property: JsonPropertyName("status")]
    string Status,
    [property: JsonPropertyName("version")]
    int Version,
    [property: JsonPropertyName("order_items")]
    List<OrderItems> OrderItems);