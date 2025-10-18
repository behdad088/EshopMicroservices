namespace Order.Query.Data.Events;

public abstract record Event
{
    public abstract string StreamId { get; }
    
    public abstract string EventType { get; }
    
    public abstract string CreatedAt { get; set; }
}