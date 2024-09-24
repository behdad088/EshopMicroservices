namespace Order.Command.Domain.Abstractions;

public interface IAggregate<T> : IAggregate, IEntity<T> where T : class;

public interface IAggregate : IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    IDomainEvent[] ClearDomainEvents();
}