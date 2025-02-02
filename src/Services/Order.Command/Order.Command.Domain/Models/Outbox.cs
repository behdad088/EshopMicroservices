using System.Reflection;
using System.Text.Json;

namespace Order.Command.Domain.Models;

public class Outbox : Aggregate<OutboxId>
{
    public AggregateId AggregateId { get; private set; } = default!;
    public AggregateType AggregateType { get; private set; } = default!;
    public DispatchDateTime DispatchDateTime { get; private set; } = default!;
    public VersionId VersionId { get; private set; } = default!;
    public IsDispatched IsDispatched { get; private set; } = default!;
    public NumberOfDispatchTry NumberOfDispatchTry { get; private set; } = default!;
    public EventType EventType { get; private set; } = default!;
    public Payload Payload { get; private set; } = default!;

    public Outbox Create(
        AggregateId aggregateId,
        AggregateType aggregateType,
        VersionId versionId,
        DispatchDateTime dispatchDateTime,
        EventType eventType,
        Payload payload)
    {
        var outbox = new Outbox
        {
            Id = OutboxId.From(Ulid.NewUlid()),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            DispatchDateTime = dispatchDateTime,
            IsDispatched = IsDispatched.No,
            EventType = eventType,
            VersionId = versionId,
            NumberOfDispatchTry = NumberOfDispatchTry.InitialValue,
            Payload = payload
        };

        return outbox;
    }

    public void Update(
        IsDispatched isDispatched,
        DispatchDateTime dispatchDateTime,
        NumberOfDispatchTry numberOfDispatchTry)
    {
        NumberOfDispatchTry = numberOfDispatchTry;
        IsDispatched = isDispatched;
        DispatchDateTime = dispatchDateTime;
    }

    public void FailedDispatch()
    {
        NumberOfDispatchTry = NumberOfDispatchTry.Increment();
        IsDispatched = IsDispatched.No;
        DispatchDateTime = DispatchDateTime.InTwoMinutes();
    }

    public void SuccessfulDispatch()
    {
        NumberOfDispatchTry = NumberOfDispatchTry.Increment();
        IsDispatched = IsDispatched.Yes;
        DispatchDateTime = DispatchDateTime.Now();
    }

    private static readonly JsonSerializerOptions
        SCaseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    public T? GetEvent<T>() where T : IDomainEvent
    {
        var domainEvent =
            JsonSerializer.Deserialize<T>(Payload.Value, SCaseInsensitiveOptions);
        return domainEvent;
    }
}