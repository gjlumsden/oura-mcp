using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Session(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("start_datetime")] DateTimeOffset? StartDatetime,
    [property: JsonPropertyName("end_datetime")] DateTimeOffset? EndDatetime,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("heart_rate")] SampleModel? HeartRate,
    [property: JsonPropertyName("heart_rate_variability")] SampleModel? HeartRateVariability,
    [property: JsonPropertyName("mood")] string? Mood,
    [property: JsonPropertyName("motion_count")] SampleModel? MotionCount
);
