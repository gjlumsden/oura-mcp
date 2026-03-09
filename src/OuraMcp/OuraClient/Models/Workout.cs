using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Workout
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("activity")] public string? Activity { get; init; }
    [JsonPropertyName("calories")] public double? Calories { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("distance")] public double? Distance { get; init; }
    [JsonPropertyName("end_datetime")] public DateTimeOffset? EndDatetime { get; init; }
    [JsonPropertyName("intensity")] public string? Intensity { get; init; }
    [JsonPropertyName("label")] public string? Label { get; init; }
    [JsonPropertyName("source")] public string? Source { get; init; }
    [JsonPropertyName("start_datetime")] public DateTimeOffset? StartDatetime { get; init; }
}
