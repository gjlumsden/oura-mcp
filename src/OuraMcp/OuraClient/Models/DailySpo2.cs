using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailySpo2(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("spo2_percentage")] Spo2AggregatedValues? Spo2Percentage,
    [property: JsonPropertyName("breathing_disturbance_index")] int? BreathingDisturbanceIndex
);

public record Spo2AggregatedValues(
    [property: JsonPropertyName("average")] double? Average
);
