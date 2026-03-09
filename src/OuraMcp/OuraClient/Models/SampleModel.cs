using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>
/// Time-series sample data used by activity MET, sleep heart rate/HRV, and session data.
/// </summary>
public record SampleModel
{
    [JsonPropertyName("interval")] public double? Interval { get; init; }
    [JsonPropertyName("items")] public IReadOnlyList<double?>? Items { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}
