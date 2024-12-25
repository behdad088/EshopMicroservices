using Order.Command.Domain.Models;

namespace Order.Command.Domain.Events;

public record OrderDeletedEvent(OrderId OrderId, DeleteDate DeleteDate, VersionId VersionId) : IDomainEvent;