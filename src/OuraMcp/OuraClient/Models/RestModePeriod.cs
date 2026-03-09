using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura rest mode period spanning a date range with associated episodes.</summary>
public record RestModePeriod
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("end_day")] public DateOnly? EndDay { get; init; }
    [JsonPropertyName("end_time")] public DateTimeOffset? EndTime { get; init; }
    [JsonPropertyName("episodes")] public IReadOnlyList<RestModeEpisode>? Episodes { get; init; }
    [JsonPropertyName("start_day")] public DateOnly? StartDay { get; init; }
    [JsonPropertyName("start_time")] public DateTimeOffset? StartTime { get; init; }
}

/// <summary>Individual episode within a rest mode period.</summary>
public record RestModeEpisode
{
    [JsonPropertyName("tags")] public IReadOnlyList<string>? Tags { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
