using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Command.Domain.Abstractions;

public class Aggregate<T> : Entity<T>, IAggregate<T> where T : class
{
    private readonly List<IDomainEvent> _domainEvents = [];

    [NotMapped] public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IDomainEvent[] ClearDomainEvents()
    {
        var dequeuedEvents = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeuedEvents;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}