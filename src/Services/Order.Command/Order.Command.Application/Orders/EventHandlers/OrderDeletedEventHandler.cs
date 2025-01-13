namespace Order.Command.Application.Orders.EventHandlers;

public class OrderDeletedEventHandler(ILogger<OrderDeletedEventHandler> logger) : INotificationHandler<OrderDeletedEvent>
{
    public async Task Handle(OrderDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain event handled: {DomainEvent}", notification.GetType());
        
        await Task.CompletedTask;
    }
}