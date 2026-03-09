using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyResilience(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("contributors")] ResilienceContributors? Contributors,
    [property: JsonPropertyName("level")] string? Level
);

public record ResilienceContributors(
    [property: JsonPropertyName("sleep_recovery")] double? SleepRecovery,
    [property: JsonPropertyName("daytime_recovery")] double? DaytimeRecovery,
    [property: JsonPropertyName("stress")] double? Stress
);
