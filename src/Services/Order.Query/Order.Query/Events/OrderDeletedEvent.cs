using System.Text.Json.Serialization;

namespace Order.Query.Events;

public record OrderDeletedEvent(
    [property: JsonPropertyName("order_id")]
    string OrderId,
    [property: JsonPropertyName("deleted_date")]
    string? DeletedDate,
    [property: JsonPropertyName("version")]
    int? Version) : Event
{
    public override string StreamId { get; } = OrderId;
    
    [property: JsonPropertyName("event_type")]
    public override string EventType => null!;

    [property: JsonPropertyName("created_at")]
    public override string CreatedAt { get; set; } = null!;
}