namespace Basket.API.ApiClient;

public record CreateOrderRequest
{
    [property: JsonPropertyName("order_name")]
    public string? OrderName { get; set; }

    [property: JsonPropertyName("shipping_Address")]
    public ModuleAddress? ShippingAddress { get; set; }

    [property: JsonPropertyName("billing_address")]
    public ModuleAddress? BillingAddress { get; set; }

    [property: JsonPropertyName("payment")]
    public ModulePayment? Payment { get; set; }

    [property: JsonPropertyName("order_items")]
    public List<ModuleOrderItem>? OrderItems { get; set; }

    public record ModuleAddress(
        [property: JsonPropertyName("firstname")]
        string? Firstname,
        [property: JsonPropertyName("lastname")]
        string? Lastname,
        [property: JsonPropertyName("email_address")]
        string? EmailAddress,
        [property: JsonPropertyName("address_line")]
        string? AddressLine,
        [property: JsonPropertyName("country")]
        string? Country,
        [property: JsonPropertyName("state")]
        string? State,
        [property: JsonPropertyName("zip_code")]
        string? ZipCode);

    public record ModulePayment(
        [property: JsonPropertyName("card_name")]
        string? CardName,
        [property: JsonPropertyName("card_number")]
        string? CardNumber,
        [property: JsonPropertyName("expiration")]
        string? Expiration,
        [property: JsonPropertyName("cvv")] 
        string? Cvv,
        [property: JsonPropertyName("payment_method")]
        int? PaymentMethod);

    public record ModuleOrderItem(
        [property: JsonPropertyName("product_id")]
        string ProductId,
        [property: JsonPropertyName("quantity")]
        int? Quantity,
        [property: JsonPropertyName("price")]
        decimal? Price);
};

public record CreateOrderResponse(
    [property: JsonPropertyName("order_id")]
    string? Id);