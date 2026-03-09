using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailySleep(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("contributors")] SleepContributors? Contributors,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);

public record SleepContributors(
    [property: JsonPropertyName("deep_sleep")] int? DeepSleep,
    [property: JsonPropertyName("efficiency")] int? Efficiency,
    [property: JsonPropertyName("latency")] int? Latency,
    [property: JsonPropertyName("rem_sleep")] int? RemSleep,
    [property: JsonPropertyName("restfulness")] int? Restfulness,
    [property: JsonPropertyName("timing")] int? Timing,
    [property: JsonPropertyName("total_sleep")] int? TotalSleep
);
