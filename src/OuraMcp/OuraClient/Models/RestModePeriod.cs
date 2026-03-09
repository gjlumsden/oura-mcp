using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record RestModePeriod(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("end_day")] DateOnly? EndDay,
    [property: JsonPropertyName("end_time")] DateTimeOffset? EndTime,
    [property: JsonPropertyName("episodes")] IReadOnlyList<RestModeEpisode>? Episodes,
    [property: JsonPropertyName("start_day")] DateOnly? StartDay,
    [property: JsonPropertyName("start_time")] DateTimeOffset? StartTime
);

public record RestModeEpisode(
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);
