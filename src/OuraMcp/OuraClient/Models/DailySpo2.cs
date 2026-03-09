using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailySpo2
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("spo2_percentage")] public Spo2AggregatedValues? Spo2Percentage { get; init; }
    [JsonPropertyName("breathing_disturbance_index")] public int? BreathingDisturbanceIndex { get; init; }
}

public record Spo2AggregatedValues
{
    [JsonPropertyName("average")] public double? Average { get; init; }
}
