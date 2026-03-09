using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura enhanced tag with typed tag code, time range, and optional comment.</summary>
public record EnhancedTag
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("tag_type_code")] public string? TagTypeCode { get; init; }
    [JsonPropertyName("start_time")] public DateTimeOffset? StartTime { get; init; }
    [JsonPropertyName("end_time")] public DateTimeOffset? EndTime { get; init; }
    [JsonPropertyName("start_day")] public DateOnly? StartDay { get; init; }
    [JsonPropertyName("end_day")] public DateOnly? EndDay { get; init; }
    [JsonPropertyName("comment")] public string? Comment { get; init; }
    [JsonPropertyName("custom_name")] public string? CustomName { get; init; }
}
