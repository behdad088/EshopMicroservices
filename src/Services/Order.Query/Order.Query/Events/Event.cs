namespace Order.Query.Events;

public abstract record Event
{
    public abstract string StreamId { get; }

    public abstract string EventType { get; set; }

    public abstract string CreatedAt { get; set; }
}
