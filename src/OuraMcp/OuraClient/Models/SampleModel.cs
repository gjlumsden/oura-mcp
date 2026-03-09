using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>
/// Time-series sample data used by activity MET, sleep heart rate/HRV, and session data.
/// </summary>
public record SampleModel(
    [property: JsonPropertyName("interval")] double? Interval,
    [property: JsonPropertyName("items")] IReadOnlyList<double?>? Items,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);
