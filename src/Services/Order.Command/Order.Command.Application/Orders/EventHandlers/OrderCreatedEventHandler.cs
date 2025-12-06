using Order.Command.Application.Rmq;

namespace Order.Command.Application.Orders.EventHandlers;

public class OrderCreatedEventHandler(
    IEventPublisher<OrderCreatedEvent> eventPublisher,
    IApplicationDbContext context)
    : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger _logger = Log.ForContext<OrderCreatedEventHandler>();
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.EventDomainType, notification.GetType());
        
        _logger.Information("Publishing domain event");
        var isPublished = await eventPublisher.PublishAsync(notification, cancellationToken).ConfigureAwait(false);

        if (isPublished)
        {
            _logger.Information("Successfully published event");

            var @event = await context.Outboxes.FirstOrDefaultAsync(x =>
                    x.AggregateId.Equals(AggregateId.From(notification.Id)) &&
                    x.VersionId.Equals(VersionId.From(notification.Version!.Value)), cancellationToken)
                .ConfigureAwait(false);
                
            @event?.Update(isDispatched: IsDispatched.Yes,
                dispatchDateTime: DispatchDateTime.Now(),
                numberOfDispatchTry: @event.NumberOfDispatchTry.Increment());
            context.Outboxes.Update(@event!);
            
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.Information("Order was marked as dispatched");
        }
    }
}