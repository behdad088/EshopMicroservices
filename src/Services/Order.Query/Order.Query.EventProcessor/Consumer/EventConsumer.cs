using System.Text.Json;
using FluentValidation;
using Marten;
using MassTransit;
using Order.Query.Events;
using Order.Query.Projections;
using Event = Order.Query.Events.Event;

namespace Order.Query.EventProcessor.Consumer;

public sealed class EventConsumer<TView, TEvent> (
    IDocumentSession session,
    IValidator<TEvent> validator): IConsumer<CloudEvent<TEvent>>
    where TEvent : Event
    where TView : class, IProjection<TView, TEvent>
{
    public async Task Consume(ConsumeContext<CloudEvent<TEvent>> context)
    {
        var cancellationTokenToken = context.CancellationToken;
        var @event = context.Message.Data;
        var source = context.Message.Source;
        var eventDocument = TView.CreateView(@event);
        
        var isValidatorResult = await validator.ValidateAsync(@event, cancellationTokenToken);
        if (!isValidatorResult.IsValid)
        {
            var validationErrors = isValidatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            throw new Exception($"Validation Failed due to: {string.Join(',', validationErrors)}");
        }
        
        var view = await session.LoadAsync<TView>(eventDocument.Id, cancellationTokenToken).ConfigureAwait(false);

        view ??= eventDocument;

        if (!view.CanUpdate(@event))
        {
            return;
        }
        
        var eventStream = GetEventStream(@event, source);
        
        view.Apply(@event);
        session.Store(view);
        session.Store(eventStream);
        await session.SaveChangesAsync(cancellationTokenToken).ConfigureAwait(false);
    }

    private static EventStream GetEventStream(TEvent @event, string source)
    {
        return new EventStream
        {
            ViewId = @event.StreamId,
            EventType = @event.EventType,
            Source = source,
            Data = JsonSerializer.Serialize(@event)
        };
    }
}