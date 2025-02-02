using Order.Command.Application.Rmq;

namespace Order.Command.Application.Orders.EventHandlers;

public class OrderDeletedEventHandler(
    ILogger<OrderDeletedEventHandler> logger,
    IEventPublisher<OrderDeletedEvent> eventPublisher,
    IApplicationDbContext context) : INotificationHandler<OrderDeletedEvent>
{
    public async Task Handle(OrderDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing domain event: {DomainEvent}", notification.GetType());
        var isPublished = await eventPublisher.PublishAsync(notification, cancellationToken).ConfigureAwait(false);

        if (isPublished)
        {
            logger.LogInformation("Publishing domain event: {DomainEvent} was successfully published",
                notification.GetType());
            
            var @event = await context.Outboxes.FirstOrDefaultAsync(x =>
                    x.AggregateId.Equals(AggregateId.From(notification.OrderId)) &&
                    x.VersionId.Equals(VersionId.From(notification.Version!.Value)), cancellationToken)
                .ConfigureAwait(false);

            @event?.Update(isDispatched: IsDispatched.Yes,
                dispatchDateTime: DispatchDateTime.Now(),
                numberOfDispatchTry: @event.NumberOfDispatchTry.Increment());
            context.Outboxes.Update(@event!);
            
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}