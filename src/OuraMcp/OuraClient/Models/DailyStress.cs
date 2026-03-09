using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura daily stress summary with high-stress and recovery durations.</summary>
public record DailyStress
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("stress_high")] public int? StressHigh { get; init; }
    [JsonPropertyName("recovery_high")] public int? RecoveryHigh { get; init; }
    [JsonPropertyName("day_summary")] public string? DaySummary { get; init; }
}
