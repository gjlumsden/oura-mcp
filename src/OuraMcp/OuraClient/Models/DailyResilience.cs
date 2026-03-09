using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyResilience
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("contributors")] public ResilienceContributors? Contributors { get; init; }
    [JsonPropertyName("level")] public string? Level { get; init; }
}

public record ResilienceContributors
{
    [JsonPropertyName("sleep_recovery")] public double? SleepRecovery { get; init; }
    [JsonPropertyName("daytime_recovery")] public double? DaytimeRecovery { get; init; }
    [JsonPropertyName("stress")] public double? Stress { get; init; }
}
