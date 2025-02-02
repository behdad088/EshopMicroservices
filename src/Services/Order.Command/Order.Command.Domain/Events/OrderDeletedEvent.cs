using System.Text.Json.Serialization;
using Order.Command.Domain.Models;

namespace Order.Command.Domain.Events;

public record OrderDeletedEvent(
    [property: JsonPropertyName("order_id")]
    Ulid OrderId,
    [property: JsonPropertyName("deleted_date")]
    string? DeletedDate,
    [property: JsonPropertyName("version")]
    int? Version) : IDomainEvent
{
    [property: JsonPropertyName("created_at")]
    public DateTimeOffset OccurredAt => DateTimeOffset.Now;

    [property: JsonPropertyName("event_type")]
    public string? EventType => GetType().AssemblyQualifiedName;
}