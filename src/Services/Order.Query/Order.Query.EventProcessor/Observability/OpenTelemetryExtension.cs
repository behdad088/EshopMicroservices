using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Order.Query.Events;

namespace Order.Query.EventProcessor.Observability;

public static class OpenTelemetryExtension
{
    private static readonly TraceContextPropagator TraceContextPropagator = new();

    public static Activity? StartCloudEventConsumerActivity<TEvent>(
        this ActivitySource activitySource,
        CloudEvent<TEvent> cloudEvent) where TEvent : Event
    {
        var spanAttributes = GetSpanAttributes(cloudEvent);
        var cloudEventTraceContext = CreateParentTraceContext(cloudEvent);
        
        var activity = activitySource.StartActivity(
            ActivityKind.Consumer,
            parentContext: cloudEventTraceContext.ActivityContext,
            tags: new ActivityTagsCollection(spanAttributes),
            name: $"Processing CloudEvents {cloudEvent.Type}");
        return activity;
    }

    /// <summary>
    ///     Extract the PropagationContext of the upstream parent from the message headers
    /// </summary>
    /// <param name="cloudEvent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static PropagationContext CreateParentTraceContext<TEvent>(
        CloudEvent<TEvent> cloudEvent)  where TEvent : Event
    {
        var parentContext = TraceContextPropagator.Extract(default, cloudEvent, GetParentTraceContext);
        Baggage.Current = parentContext.Baggage;
        
        return parentContext;
    }

    private static IEnumerable<string> GetParentTraceContext<TEvent>(
        CloudEvent<TEvent> carrier,
        string key) where TEvent : Event
    {
        return key switch
        {
            W3CContextStandard.TraceParent => carrier.TraceParent == null ? [] : [carrier.TraceParent!],
            W3CContextStandard.TraceState => carrier.TraceState == null ? [] : [carrier.TraceState],
            _ => throw new InvalidOperationException($"Invalid W3C trace-context header '{key}'. Expected 'traceparent' or 'tracestate'.")
        };
    }

    private static IEnumerable<KeyValuePair<string, object?>>
        GetSpanAttributes<TEvent>(CloudEvent<TEvent> cloudEvent)
        where TEvent : Event
    {
        var attributes = new Dictionary<string, string?>
        {
            [SpanTags.CloudEventId] = cloudEvent.Id,
            [SpanTags.CloudEventSource] = cloudEvent.Source,
            [SpanTags.CloudEventSpecVersion] = cloudEvent.SpecVersion,
            [SpanTags.CloudEventType] = cloudEvent.Type
        };

        var attributeTags = attributes.Select(a =>
            new KeyValuePair<string, object?>(a.Key, a.Value));

        return attributeTags;
    }

    private static class SpanTags
    {
        public const string CloudEventId = "cloudevents.event_id";
        public const string CloudEventSource = "cloudevents.event_source";
        public const string CloudEventSpecVersion = "cloudevents.event_spec_version";
        public const string CloudEventType = "cloudevents.event_type";
    }

    private static class W3CContextStandard
    {
        public const string TraceParent = "traceparent";
        public const string TraceState = "tracestate";
    }
}