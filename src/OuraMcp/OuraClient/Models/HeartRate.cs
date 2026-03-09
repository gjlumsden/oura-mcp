using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record HeartRate(
    [property: JsonPropertyName("bpm")] int? Bpm,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);
