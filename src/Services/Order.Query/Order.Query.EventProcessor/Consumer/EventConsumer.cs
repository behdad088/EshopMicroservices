using System.Diagnostics;
using System.Text.Json;
using eshop.Shared.Logger;
using eshop.Shared.OpenTelemetry;
using FluentValidation;
using Marten;
using MassTransit;
using Order.Query.EventProcessor.Observability;
using Order.Query.Events;
using Order.Query.Projections;
using Serilog;
using Event = Order.Query.Events.Event;
using ILogger = Serilog.ILogger;
using static OpenTelemetry.Trace.TraceSemanticConventions;

namespace Order.Query.EventProcessor.Consumer;

public sealed class EventConsumer<TView, TEvent> (
    Telemetry telemetry,
    IDocumentSession session,
    IValidator<TEvent> validator): IConsumer<CloudEvent<TEvent>>
    where TEvent : Event
    where TView : class, IProjection<TView, TEvent>
{
    private readonly ILogger _logger = Log.ForContext<EventConsumer<TView, TEvent>>();
    public async Task Consume(ConsumeContext<CloudEvent<TEvent>> context)
    {
        using var messageSpan = telemetry.Tracing.StartActivity(
            name: $"message received {context.Message.Type}", 
            kind: ActivityKind.Consumer);
        
        var cancellationTokenToken = context.CancellationToken;
        var @event = context.Message.Data;
        if (@event is null)
        {
            _logger.Warning("Received event data is null. Message Id: {messageId}", context.Message.Id);
            throw new ValidationException($"Received event data is null. Message Id: {context.Message.Id}");
        }
        
        var source = context.Message.Source;
        var eventDocument = TView.CreateView(@event);
        
        using var activity = telemetry.Tracing.StartCloudEventConsumerActivity(context.Message);
        
        EnrichLoggerScope(context, @event);
        
        _logger.Information("Processing event: {message}", @event);
        
        messageSpan?.SetTag(AttributeMessagingMessageId, @event.StreamId);
        messageSpan?.SetTag($"{PrefixMessaging}.is_retry", context.GetRetryAttempt() > 0);

        await AssertEvent(@event, messageSpan, cancellationTokenToken);
        
        var view = await session.LoadAsync<TView>(eventDocument.Id, cancellationTokenToken).ConfigureAwait(false);

        view ??= eventDocument;

        if (!view.CanUpdate(@event))
        {
            _logger.Information("Stop processing event due to the outdated event.");
            return;
        }
        
        var eventStream = GetEventStream(@event, source!);
        
        view.Apply(@event);
        session.Store(view);
        session.Store(eventStream);
        await session.SaveChangesAsync(cancellationTokenToken).ConfigureAwait(false);
        
        _logger.Information("Successfully processed event: {message}", @event);
    }
    
    private static void EnrichLoggerScope(ConsumeContext<CloudEvent<TEvent>> context, TEvent @event)
    {
        using var _ = Serilog.Context.LogContext.PushProperty(LogProperties.EventId, context.Message.Id);
        using var __ = Serilog.Context.LogContext.PushProperty(LogProperties.EventType, @event.EventType);
        using var ___ = Serilog.Context.LogContext.PushProperty(LogProperties.EventSource, context.Message.Source);
        using var ____ = Serilog.Context.LogContext.PushProperty(LogProperties.StreamId, @event.StreamId);
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
    
    private async Task AssertEvent(TEvent @event, Activity? activity, CancellationToken ct)
    {
        var isValidatorResult = await validator.ValidateAsync(@event, ct);
        if (!isValidatorResult.IsValid)
        {
            var validationErrors = isValidatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            
            activity?.SetTag($"{PrefixException}.validation", validationErrors);
            _logger.Error("Processing the message failed due to the validation failure: {ValidationErrors}",
                string.Join(Environment.NewLine, validationErrors));
            
            throw new ValidationException($"Validation Failed due to: {string.Join(',', validationErrors)}");
        }
    }
}