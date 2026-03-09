using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Workout(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("activity")] string? Activity,
    [property: JsonPropertyName("calories")] double? Calories,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("distance")] double? Distance,
    [property: JsonPropertyName("end_datetime")] DateTimeOffset? EndDatetime,
    [property: JsonPropertyName("intensity")] string? Intensity,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("start_datetime")] DateTimeOffset? StartDatetime
);
