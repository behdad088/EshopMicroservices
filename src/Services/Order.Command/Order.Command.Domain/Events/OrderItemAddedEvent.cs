using Order.Command.Domain.Models;

namespace Order.Command.Domain.Events;

public record OrderItemAddedEvent(OrderItem OrderItem) : IDomainEvent;