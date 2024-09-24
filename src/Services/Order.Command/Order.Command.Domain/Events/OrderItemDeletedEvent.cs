using Order.Command.Domain.Models;

namespace Order.Command.Domain.Events;

public record OrderItemDeletedEvent(OrderItem OrderItem) : IDomainEvent;