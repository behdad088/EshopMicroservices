using System.Text.Json.Serialization;

namespace Order.Command.Domain.Events;

public record OrderUpdatedEvent(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("last_modified")]
    DateTimeOffset? LastModified,
    [property: JsonPropertyName("customer_id")]
    Guid CustomerId,
    [property: JsonPropertyName("order_name")]
    string? OrderName,
    [property: JsonPropertyName("order_items")]
    List<OrderCreatedEvent.OrderItem> OrderItems,
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
) : IDomainEvent
{
    [property: JsonPropertyName("created_at")]
    public DateTimeOffset OccurredAt => DateTimeOffset.Now;

    [property: JsonPropertyName("event_type")]
    public string EventType => "order_updated_event";

    public record OrderItem(
        [property: JsonPropertyName("id")] Ulid Id,
        [property: JsonPropertyName("order_id")]
        Ulid OrderId,
        [property: JsonPropertyName("product_id")]
        Ulid ProductId,
        [property: JsonPropertyName("quantity")]
        int Quantity,
        [property: JsonPropertyName("price")] decimal Price
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
        int PaymentMethod);
}