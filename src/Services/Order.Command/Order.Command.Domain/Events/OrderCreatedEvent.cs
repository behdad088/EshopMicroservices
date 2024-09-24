using Order.Command.Domain.Models;

namespace Order.Command.Domain.Events;

public record OrderCreatedEvent(Models.Order Order) : IDomainEvent;