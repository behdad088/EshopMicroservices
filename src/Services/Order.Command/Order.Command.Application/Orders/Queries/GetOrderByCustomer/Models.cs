using System.Text.Json.Serialization;

namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public record GetOrderByCustomerParameter(
    [property: JsonPropertyName("id")] 
    Ulid Id,
    [property: JsonPropertyName("customer_id")]
    Guid CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    AddressParameter ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    AddressParameter BillingAddress,
    [property: JsonPropertyName("payment")]
    PaymentParameter Payment,
    [property: JsonPropertyName("status")] 
    string Status,
    [property: JsonPropertyName("order_items")]
    List<OrderItemParameter> OrderItems);

public record OrderItemParameter(
    [property: JsonPropertyName("id")] 
    string Id,
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] 
    decimal? Price);

public record AddressParameter(
    [property: JsonPropertyName("firstname")]
    string Firstname,
    [property: JsonPropertyName("lastname")]
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
    string ZipCode);

public record PaymentParameter(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration")]
    string Expiration,
    [property: JsonPropertyName("cvv")] 
    string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);