using System.Text.Json.Serialization;
using MassTransit;

namespace Order.Query.Data.Events;

public record CloudEvent<TEvent> where TEvent : Event
{
    [JsonPropertyName("id")] public string Id { get; init; }
    [JsonPropertyName("type")] public string Type { get; init; }
    [JsonPropertyName("source")] public string Source { get; init; }
    [JsonPropertyName("specversion")] public string SpecVersion { get; init; }
    [JsonPropertyName("datacontenttype")] public string DataContentType { get; init; }

    [JsonPropertyName("dataschema")] public Uri DataSchema { get; init; }

    [JsonPropertyName("subject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subject { get; init; }

    [JsonPropertyName("time")] public DateTimeOffset Time { get; init; }
    [JsonPropertyName("data")] public TEvent Data { get; init; }
    [JsonPropertyName("traceparent")] public string? TraceParent { get; set; }
    [JsonPropertyName("tracestate")] public string? TraceState { get; set; }
}