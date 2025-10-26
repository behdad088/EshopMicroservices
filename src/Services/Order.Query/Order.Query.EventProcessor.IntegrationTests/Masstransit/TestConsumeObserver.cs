using System.Collections.Concurrent;
using MassTransit;
using MassTransit.Testing;
using Order.Query.Events;
using Event = Order.Query.Events.Event;

namespace Order.Query.EventProcessor.IntegrationTests.Masstransit;

public class TestConsumeObserver(ITestHarness testHarness) : IConsumeObserver
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<(ConsumeContext Context, Exception? Exception)>> _tcs
        = new();
    private void WatchMessage(Guid messageId)
    {
        _tcs.TryAdd(messageId,
            new TaskCompletionSource<(ConsumeContext, Exception?)>(TaskCreationOptions.RunContinuationsAsynchronously));
    }

    public async Task<(ConsumeContext, Exception?)> PublishEventAndWaitForConsumed<T>(
        CloudEvent<T> @event,
        Guid messageId,
        CancellationToken cancellationToken)
    where T : Event
    {
        WatchMessage(messageId);
        if (!_tcs.TryGetValue(messageId, out var tcs))
            return await Task.FromException<(ConsumeContext, Exception?)>(
                new InvalidOperationException("MessageId not registered"));
        
        var ct = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        testHarness.Bus.ConnectConsumeObserver(this);
        await testHarness.Bus.Publish(@event, context => context.MessageId = messageId, ct.Token);
        
        ct.Token.Register(() => tcs.TrySetException(new TimeoutException("Timed out waiting for consumed")),
            useSynchronizationContext: false);

        return await tcs.Task;
    }
    
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        if (context.MessageId.HasValue && _tcs.TryGetValue(context.MessageId.Value, out var tcs))
            tcs.TrySetResult((context, null));

        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        if (context.MessageId.HasValue && _tcs.TryGetValue(context.MessageId.Value, out var tcs))
            tcs.TrySetResult((context, exception));

        return Task.CompletedTask;
    }
}