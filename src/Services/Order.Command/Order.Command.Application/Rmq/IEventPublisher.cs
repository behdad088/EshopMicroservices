namespace Order.Command.Application.Rmq;

public interface IEventPublisher<in T>
{
    Task<bool> PublishAsync(T @event, CancellationToken cancellationToken = default);
}