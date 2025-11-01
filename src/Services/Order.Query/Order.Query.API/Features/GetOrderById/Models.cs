using System.Text.Json.Serialization;
using FastEndpoints;

namespace Order.Query.API.Features.GetOrderById;

public record Request(
    [property: BindFrom("CustomerId")]
    string? CustomerId,
    [property: BindFrom("OrderId")]
    string? OrderId);

public record Response(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("customer_id")]
    string CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_address")]
    Address ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    Address BillingAddress,
    [property: JsonPropertyName("payment")]
    Payment Payment,
    [property: JsonPropertyName("status")]
    string Status,
    [property: JsonPropertyName("version")]
    int Version,
    [property: JsonPropertyName("order_items")]
    List<OrderItem> OrderItems);

public record OrderItem(
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")]
    decimal? Price
);

public record Address(
    [property: JsonPropertyName("first_name")]
    string Firstname,
    [property: JsonPropertyName("last_name")]
    string Lastname,
    [property: JsonPropertyName("email_address")]
    string EmailAddress,
    [property: JsonPropertyName("address_line")]
    string AddressLine,
    [property: JsonPropertyName("country")]
    string Country,
    [property: JsonPropertyName("state")]
    string State,
    [property: JsonPropertyName("zip_code")]
    string ZipCode
);

public record Payment(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration_date")]
    string Expiration,
    [property: JsonPropertyName("cvv")]
    string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);