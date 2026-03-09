using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Vo2Max
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
    [JsonPropertyName("vo2_max")] public double? Vo2MaxValue { get; init; }
}
