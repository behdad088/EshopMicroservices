namespace Order.Command.API.Endpoints.CreateOrder;

public record Request
{
    [property: JsonPropertyName("id")] public string Id { get; set; }

    [property: JsonPropertyName("customer_id")]
    public string CustomerId { get; set; }

    [property: JsonPropertyName("order_name")]
    public string OrderName { get; set; }

    [property: JsonPropertyName("shipping_Address")]
    public AddressDto ShippingAddress { get; set; }

    [property: JsonPropertyName("billing_address")]
    public AddressDto BillingAddress { get; set; }

    [property: JsonPropertyName("payment")]
    public PaymentDto Payment { get; set; }

    [property: JsonPropertyName("order_items")]
    public List<OrderItemsDto> OrderItems { get; set; }
};

public record AddressDto(
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

public record PaymentDto(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration")]
    string Expiration,
    [property: JsonPropertyName("cvv")] string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);

public record OrderItemsDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("order_id")]
    string OrderId,
    [property: JsonPropertyName("product_id")]
    string ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);

public record Response(
    [property: JsonPropertyName("order_id")]
    Ulid Id);