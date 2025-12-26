using System.Text.Json.Serialization;

namespace Order.Query.API.Features.GetOrdersByName;

public record Request(
    [property: BindFrom("customer_name")] 
    string? OrderName,
    [property: BindFrom("page_size")]
    int PageSize = 10,
    [property: BindFrom("page_index")] 
    int PageIndex = 0);


public record Response(
    [property: JsonPropertyName("data")] 
    PaginatedItems<Order> Orders);

public record Order(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("customer_id")]
    string CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    Address ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    Address BillingAddress,
    [property: JsonPropertyName("payment")]
    Payment Payment,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("order_items")]
    List<OrderItem> OrderItems);

public record OrderItem(
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);

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