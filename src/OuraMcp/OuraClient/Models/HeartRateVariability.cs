using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura daily heart rate variability data containing timestamped HRV items.</summary>
public record HeartRateVariability
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
    [JsonPropertyName("items")] public IReadOnlyList<HrvItem>? Items { get; init; }
}

/// <summary>Individual HRV measurement with RMSSD and SDNN averages.</summary>
public record HrvItem
{
    [JsonPropertyName("avg_rmssd")] public double? AvgRmssd { get; init; }
    [JsonPropertyName("avg_sdnn")] public double? AvgSdnn { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
