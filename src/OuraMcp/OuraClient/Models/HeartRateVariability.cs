using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record HeartRateVariability(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp,
    [property: JsonPropertyName("items")] IReadOnlyList<HrvItem>? Items
);

public record HrvItem(
    [property: JsonPropertyName("avg_rmssd")] double? AvgRmssd,
    [property: JsonPropertyName("avg_sdnn")] double? AvgSdnn,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);
