using MassTransit;
using Order.Command.Application.Rmq.CloudEvent;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application.Rmq;

public delegate Task<bool> PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default);

internal class EventPublisher<TEvent>(
    ILogger<EventPublisher<TEvent>> logger,
    ICloudEventFactory<TEvent> cloudEventFactory,
    IBus bus) : IEventPublisher<TEvent> where TEvent : class, IDomainEvent
{
    public async Task<bool> PublishAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = cloudEventFactory.Create(@event);
            await bus.Publish(message, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Something went wrong when trying to publish event {typeof(TEvent)}");
            return false;
        }
    }
}