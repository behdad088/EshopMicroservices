using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.UpdateOrder;

public record Request
{
    [property: JsonPropertyName("id")] public string Id { get; set; }

    [property: JsonPropertyName("customer_id")]
    public string? CustomerId { get; set; }

    [property: JsonPropertyName("order_name")]
    public string OrderName { get; set; }

    [property: JsonPropertyName("shipping_Address")]
    public Address ShippingAddress { get; set; }

    [property: JsonPropertyName("billing_address")]
    public Address BillingAddress { get; set; }

    [property: JsonPropertyName("payment")]
    public Payment Payment { get; set; }

    [property: JsonPropertyName("status")] public string Status { get; set; }

    [property: JsonPropertyName("order_items")]
    public List<OrderItem> OrderItems { get; set; }
}

public record Address(
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

public record Payment(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration")]
    string Expiration,
    [property: JsonPropertyName("cvv")] string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);

public record OrderItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("order_id")]
    string OrderId,
    [property: JsonPropertyName("product_id")]
    string ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);

public record Response(
    [property: JsonPropertyName("success")]
    bool IsSuccess);