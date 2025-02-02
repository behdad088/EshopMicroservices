using Order.Command.Application.Rmq;

namespace Order.Command.Application.Orders.EventHandlers;

public class OrderUpdatedEventHandler(
    ILogger<OrderUpdatedEventHandler> logger,
    IEventPublisher<OrderUpdatedEvent> eventPublisher,
    IApplicationDbContext context)
    : INotificationHandler<OrderUpdatedEvent>
{
    public async Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
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