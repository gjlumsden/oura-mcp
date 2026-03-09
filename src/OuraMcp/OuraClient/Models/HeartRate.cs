using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record HeartRate
{
    [JsonPropertyName("bpm")] public int? Bpm { get; init; }
    [JsonPropertyName("source")] public string? Source { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
