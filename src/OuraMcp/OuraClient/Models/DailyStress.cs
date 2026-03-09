using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyStress(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("stress_high")] int? StressHigh,
    [property: JsonPropertyName("recovery_high")] int? RecoveryHigh,
    [property: JsonPropertyName("day_summary")] string? DaySummary
);
