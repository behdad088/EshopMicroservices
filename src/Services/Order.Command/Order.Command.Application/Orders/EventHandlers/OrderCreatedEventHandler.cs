using System.Text.Json;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application.Orders.EventHandlers;

public class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain event handled: {DomainEvent}", notification.GetType());
        await Task.CompletedTask;
    }
}