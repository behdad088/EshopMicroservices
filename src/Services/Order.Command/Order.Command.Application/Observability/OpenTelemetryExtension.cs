using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Order.Command.Application.Rmq.CloudEvent.Models;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application.Observability;

public static class OpenTelemetryExtension
{
    private static readonly TraceContextPropagator TraceContextPropagator = new();
    
    public static Activity? StartCloudEventPublisherActivity<TEvent>(
        this ActivitySource activitySource,
        CloudEvent<TEvent> cloudEvent) where TEvent : IDomainEvent
    {
        var spanAttributes = GetSpanAttributes(cloudEvent);
        var activity = activitySource.StartActivity(
            kind: ActivityKind.Producer,
            tags: new ActivityTagsCollection(spanAttributes),
            name: $"Publishing CloudEvents {cloudEvent.Type}");
        
        SetTraceContext(activity, cloudEvent);
        return activity;
    }

    private static void SetTraceContext<TEvent>(
        Activity? activity,
        CloudEvent<TEvent> cloudEvent) where TEvent : IDomainEvent
    {
        if (activity is null)
            return;
        
        TraceContextPropagator.Inject(
            new PropagationContext(activity.Context, Baggage.Current), 
            cloudEvent,
            SetTraceContextInCloudEvent);
    }
    
    private static void SetTraceContextInCloudEvent<TEvent>(
        CloudEvent<TEvent> cloudEvent,
        string key,
        string value) where TEvent : IDomainEvent
    {
        switch (key)
        {
            case W3CContextStandard.TraceParent:
                cloudEvent.TraceParent = value;
                break;
            case W3CContextStandard.TraceState:
                cloudEvent.TraceState = value; 
                break;
            default:
                throw new InvalidOperationException($"Invalid W3C trace-context header '{key}'. Expected 'traceparent' or 'tracestate'.");
        }
    }
    
    private static IEnumerable<KeyValuePair<string, object?>> 
        GetSpanAttributes<TEvent>(CloudEvent<TEvent> cloudEvent) 
        where TEvent : IDomainEvent
    {
        var attributes = new Dictionary<string, string>
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