using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Command.Application.Rmq;

namespace Order.Command.Application.Outbox;

public class OutboxService(
    ILogger<OutboxService> logger,
    IServiceScopeFactory serviceScopeFactory,
    IHostApplicationLifetime hostApplicationLifetime
) : BackgroundService
{
    private const int DelayInMilliseconds = 2000;
    
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            logger.LogInformation("Starting outbox");
            while (!token.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var orderCreatedEventPublisher =
                    scope.ServiceProvider.GetRequiredService<IEventPublisher<OrderCreatedEvent>>();
                var orderUpdatedEventPublisher =
                    scope.ServiceProvider.GetRequiredService<IEventPublisher<OrderUpdatedEvent>>();
                var orderDeletedEventPublisher =
                    scope.ServiceProvider.GetRequiredService<IEventPublisher<OrderDeletedEvent>>();

                var events = await GetEventsToDispatchAsync(dbContext, token);

                if (events.Count != 0)
                {
                    await DispatchEventAsync(
                        dbContext,
                        events,
                        orderCreatedEventPublisher,
                        orderUpdatedEventPublisher,
                        orderDeletedEventPublisher,
                        token).ConfigureAwait(false);
                }

                await DelayAsync(token).ConfigureAwait(false);
            }
        }
        catch when (token.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception: exception, "Unhandled exception when dispatching messages");
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task DispatchEventAsync(
        IApplicationDbContext dbContext,
        List<Domain.Models.Outbox> events,
        IEventPublisher<OrderCreatedEvent> orderCreatedEventPublisher,
        IEventPublisher<OrderUpdatedEvent> orderUpdatedEventPublisher,
        IEventPublisher<OrderDeletedEvent> orderDeletedEventPublisher,
        CancellationToken token)
    {
        foreach (var @event in events)
        {
            try
            {
                if (@event.EventType.Value == nameof(OrderCreatedEvent))
                {
                    var orderCreatedEventPayload = @event.GetEvent<OrderCreatedEvent>();

                    if (orderCreatedEventPayload != null)
                    {
                        await orderCreatedEventPublisher
                            .PublishAsync(orderCreatedEventPayload, token)
                            .ConfigureAwait(false);
                    }
                }

                if (@event.EventType.Value == nameof(OrderUpdatedEvent))
                {
                    var orderUpdatedEventPayload = @event.GetEvent<OrderUpdatedEvent>();

                    if (orderUpdatedEventPayload != null)
                    {
                        await orderUpdatedEventPublisher
                            .PublishAsync(orderUpdatedEventPayload, token)
                            .ConfigureAwait(false);
                    }
                }

                if (@event.EventType.Value == nameof(OrderDeletedEvent))
                {
                    var orderDeletedEventPayload = @event.GetEvent<OrderDeletedEvent>();

                    if (orderDeletedEventPayload != null)
                    {
                        await orderDeletedEventPublisher
                            .PublishAsync(orderDeletedEventPayload, token)
                            .ConfigureAwait(false);
                    }
                }

                @event.SuccessfulDispatch();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error when dispatching event_type {@event.EventType} with id {@event.Id} message");
                @event.FailedDispatch();
            }

            dbContext.Outboxes.Update(@event);
        }
        
        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
    }

    private static async Task<List<Domain.Models.Outbox>> GetEventsToDispatchAsync(
        IApplicationDbContext dbContext,
        CancellationToken token) =>
        await dbContext.Outboxes.Where(x => x.IsDispatched.Equals(IsDispatched.No)
                                            && x.DispatchDateTime <= DispatchDateTime.Now())
            .ToListAsync(token);

    private static Task DelayAsync(CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromMilliseconds(DelayInMilliseconds), cancellationToken);
}