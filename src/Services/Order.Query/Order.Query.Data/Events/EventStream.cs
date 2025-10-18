namespace Order.Query.Data.Events;

public record EventStream
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ViewId { get; set; }
    public string EventType { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Source { get; set; } = default!;
    public string Data { get; set; } = default!;
}