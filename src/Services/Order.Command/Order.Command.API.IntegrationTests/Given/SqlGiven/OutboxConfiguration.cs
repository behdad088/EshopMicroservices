namespace Order.Command.API.IntegrationTests.Given.SqlGiven;

public record OutboxConfiguration
{
    public AggregateId AggregateId { get; set; } = AggregateId.From(Ulid.NewUlid());
    public AggregateType AggregateType { get; set; } = AggregateType.From("TestAggregateType");
    public DispatchDateTime DispatchDateTime { get; set; } = DispatchDateTime.Now();
    public VersionId VersionId { get; set; } = VersionId.From(1);
    public IsDispatched IsDispatched { get; set; } = IsDispatched.No;
    public NumberOfDispatchTry NumberOfDispatchTry { get; set; } = NumberOfDispatchTry.InitialValue;
    public CustomerId CustomerId { get; init; } = CustomerId.From(Guid.Empty);
    public EventType EventType { get; set; } = EventType.From("TestEventType");
    public Payload? Payload { get; set; }
}

public static class CreateOutboxConfiguration
{
    public static Outbox ToOutboxDb(this OutboxConfiguration outbox)
    {
        return new Outbox().Create(
            outbox.AggregateId,
            outbox.CustomerId,
            outbox.AggregateType,
            outbox.VersionId,
            outbox.DispatchDateTime,
            outbox.EventType,
            outbox.Payload ?? Payload.From("{}"));
    }
}