using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record HeartRateVariability
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
    [JsonPropertyName("items")] public IReadOnlyList<HrvItem>? Items { get; init; }
}

public record HrvItem
{
    [JsonPropertyName("avg_rmssd")] public double? AvgRmssd { get; init; }
    [JsonPropertyName("avg_sdnn")] public double? AvgSdnn { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
