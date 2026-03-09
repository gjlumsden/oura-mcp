using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Vo2Max(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp,
    [property: JsonPropertyName("vo2_max")] double? Vo2MaxValue
);
