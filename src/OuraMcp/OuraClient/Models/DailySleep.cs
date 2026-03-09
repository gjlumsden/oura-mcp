using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura daily sleep score and timestamp for a given day.</summary>
public record DailySleep
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("contributors")] public SleepContributors? Contributors { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("score")] public int? Score { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}

/// <summary>Contributors to the daily sleep score.</summary>
public record SleepContributors
{
    [JsonPropertyName("deep_sleep")] public int? DeepSleep { get; init; }
    [JsonPropertyName("efficiency")] public int? Efficiency { get; init; }
    [JsonPropertyName("latency")] public int? Latency { get; init; }
    [JsonPropertyName("rem_sleep")] public int? RemSleep { get; init; }
    [JsonPropertyName("restfulness")] public int? Restfulness { get; init; }
    [JsonPropertyName("timing")] public int? Timing { get; init; }
    [JsonPropertyName("total_sleep")] public int? TotalSleep { get; init; }
}
