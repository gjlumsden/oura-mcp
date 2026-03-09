using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura guided or unguided session (e.g., meditation, breathing) with biometric samples.</summary>
public record Session
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("start_datetime")] public DateTimeOffset? StartDatetime { get; init; }
    [JsonPropertyName("end_datetime")] public DateTimeOffset? EndDatetime { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("heart_rate")] public SampleModel? HeartRate { get; init; }
    [JsonPropertyName("heart_rate_variability")] public SampleModel? HeartRateVariability { get; init; }
    [JsonPropertyName("mood")] public string? Mood { get; init; }
    [JsonPropertyName("motion_count")] public SampleModel? MotionCount { get; init; }
}
