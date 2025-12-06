using Microsoft.AspNetCore.Mvc;

namespace Order.Command.API.Endpoints.GetOrderById;

public record Request
{
    [FromRoute(Name = "id")] public string? Id { get; set; }
    [FromRoute(Name = "customer_id")] public string? CustomerId { get; set; }
}

public record Response(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("customer_id")]
    string? CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    ModuleAddress ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    ModuleAddress BillingAddress,
    [property: JsonPropertyName("payment")]
    ModulePayment Payment,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("order_items")]
    List<ModuleOrderItem> OrderItems);

public record ModuleOrderItem(
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] 
    decimal? Price);

public record ModuleAddress(
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
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("zip_code")]
    string ZipCode);

public record ModulePayment(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration")]
    string Expiration,
    [property: JsonPropertyName("cvv")] string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);