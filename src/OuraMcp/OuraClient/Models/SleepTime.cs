using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record SleepTime(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("optimal_bedtime")] SleepTimeWindow? OptimalBedtime,
    [property: JsonPropertyName("recommendation")] string? Recommendation,
    [property: JsonPropertyName("status")] string? Status
);

public record SleepTimeWindow(
    [property: JsonPropertyName("day_tz")] int? DayTz,
    [property: JsonPropertyName("end_offset")] int? EndOffset,
    [property: JsonPropertyName("start_offset")] int? StartOffset
);
