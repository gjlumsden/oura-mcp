using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura heart rate sample with BPM, source, and timestamp.</summary>
public record HeartRate
{
    [JsonPropertyName("bpm")] public int? Bpm { get; init; }
    [JsonPropertyName("source")] public string? Source { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
