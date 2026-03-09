using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record EnhancedTag(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("tag_type_code")] string? TagTypeCode,
    [property: JsonPropertyName("start_time")] DateTimeOffset? StartTime,
    [property: JsonPropertyName("end_time")] DateTimeOffset? EndTime,
    [property: JsonPropertyName("start_day")] DateOnly? StartDay,
    [property: JsonPropertyName("end_day")] DateOnly? EndDay,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("custom_name")] string? CustomName
);
