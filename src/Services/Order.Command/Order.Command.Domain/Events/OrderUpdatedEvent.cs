namespace Order.Command.Domain.Events;

public record OrderUpdatedEvent(Models.Order Order) : IDomainEvent;