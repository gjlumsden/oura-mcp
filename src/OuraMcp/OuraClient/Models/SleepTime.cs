using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura recommended sleep time with optimal bedtime window and status.</summary>
public record SleepTime
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("optimal_bedtime")] public SleepTimeWindow? OptimalBedtime { get; init; }
    [JsonPropertyName("recommendation")] public string? Recommendation { get; init; }
    [JsonPropertyName("status")] public string? Status { get; init; }
}

/// <summary>Optimal bedtime window expressed as start and end offsets from midnight.</summary>
public record SleepTimeWindow
{
    [JsonPropertyName("day_tz")] public int? DayTz { get; init; }
    [JsonPropertyName("end_offset")] public int? EndOffset { get; init; }
    [JsonPropertyName("start_offset")] public int? StartOffset { get; init; }
}
