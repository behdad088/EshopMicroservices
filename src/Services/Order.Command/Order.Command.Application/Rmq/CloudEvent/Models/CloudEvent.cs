using System.Text.Json.Serialization;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application.Rmq.CloudEvent.Models;

public record CloudEvent<TEvent> where TEvent : IDomainEvent
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("source")] public required string Source { get; init; }
    [JsonPropertyName("specversion")] public required string SpecVersion { get; init; }
    [JsonPropertyName("datacontenttype")] public string? DataContentType { get; init; }

    [JsonPropertyName("dataschema")] public Uri? DataSchema { get; init; }

    [JsonPropertyName("subject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subject { get; init; }

    [JsonPropertyName("time")] public DateTimeOffset Time { get; init; }
    [JsonPropertyName("data")] public required TEvent Data { get; init; }
    [JsonPropertyName("traceparent")] public string? TraceParent { get; set; }
    [JsonPropertyName("tracestate")] public string? TraceState { get; set; }
}