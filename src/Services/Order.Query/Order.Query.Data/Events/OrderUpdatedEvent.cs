using System.Text.Json.Serialization;
namespace Order.Query.Data.Events;

public record OrderUpdatedEvent(
    [property: JsonPropertyName("id")] 
    string Id,
    [property: JsonPropertyName("last_modified")]
    DateTimeOffset? LastModified,
    [property: JsonPropertyName("order_name")]
    string? OrderName,
    [property: JsonPropertyName("order_items")]
    List<OrderUpdatedEvent.OrderItem> OrderItems,
    [property: JsonPropertyName("shipping_address")]
    OrderUpdatedEvent.Address ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    OrderUpdatedEvent.Address BillingAddress,
    [property: JsonPropertyName("payment_method")]
    OrderUpdatedEvent.Payment PaymentMethod,
    [property: JsonPropertyName("order_status")]
    string? OrderStatus,
    [property: JsonPropertyName("total_price")]
    decimal TotalPrice,
    [property: JsonPropertyName("version")]
    int? Version
) : Event
{
    public override string StreamId { get; } = Id;
    
    [property: JsonPropertyName("event_type")]
    public override string EventType { get; }

    [property: JsonPropertyName("created_at")]
    public override string CreatedAt { get; set; }

    public record OrderItem(
        [property: JsonPropertyName("id")] 
        string Id,
        [property: JsonPropertyName("product_id")]
        string ProductId,
        [property: JsonPropertyName("quantity")]
        int? Quantity,
        [property: JsonPropertyName("price")] 
        decimal? Price
    );

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
        [property: JsonPropertyName("Payment_method")]
        int? PaymentMethod);
}