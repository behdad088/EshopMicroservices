using Order.Query.Events;

namespace Order.Query.EventProcessor.IntegrationTests.Given.DbGiven;

public class EventStreamConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ViewId { get; set; } = Ulid.NewUlid().ToString();
    public string EventType { get; set; } = "TestEventType";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Source { get; set; } = "Test Source";
    public string Data { get; set; } = "Event Payload";
}

public static class EventStreamConfigurationExtension
{
    public static EventStream ToDbModel(this EventStreamConfiguration configuration)
    {
        return new EventStream
        {
            Id = configuration.Id,
            ViewId = configuration.ViewId,
            EventType = configuration.EventType,
            CreatedAt = configuration.CreatedAt,
            Source = configuration.Source,
            Data = configuration.Data
        };
    }
}