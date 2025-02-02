using Order.Command.Application.Rmq;

namespace Order.Command.Application.Orders.EventHandlers;

public class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    IEventPublisher<OrderCreatedEvent> eventPublisher,
    IApplicationDbContext context)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing domain event: {DomainEvent}", notification.GetType());
        var isPublished = await eventPublisher.PublishAsync(notification, cancellationToken).ConfigureAwait(false);

        if (isPublished)
        {
            logger.LogInformation("Publishing domain event: {DomainEvent} was successfully published",
                notification.GetType());

            var @event = await context.Outboxes.FirstOrDefaultAsync(x =>
                    x.AggregateId.Equals(AggregateId.From(notification.Id)) &&
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