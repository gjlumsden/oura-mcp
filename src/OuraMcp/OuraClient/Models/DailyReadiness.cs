using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyReadiness
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("contributors")] public ReadinessContributors? Contributors { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("score")] public int? Score { get; init; }
    [JsonPropertyName("temperature_deviation")] public double? TemperatureDeviation { get; init; }
    [JsonPropertyName("temperature_trend_deviation")] public double? TemperatureTrendDeviation { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}

public record ReadinessContributors
{
    [JsonPropertyName("activity_balance")] public int? ActivityBalance { get; init; }
    [JsonPropertyName("body_temperature")] public int? BodyTemperature { get; init; }
    [JsonPropertyName("hrv_balance")] public int? HrvBalance { get; init; }
    [JsonPropertyName("previous_day_activity")] public int? PreviousDayActivity { get; init; }
    [JsonPropertyName("previous_night")] public int? PreviousNight { get; init; }
    [JsonPropertyName("recovery_index")] public int? RecoveryIndex { get; init; }
    [JsonPropertyName("resting_heart_rate")] public int? RestingHeartRate { get; init; }
    [JsonPropertyName("sleep_balance")] public int? SleepBalance { get; init; }
    [JsonPropertyName("sleep_regularity")] public int? SleepRegularity { get; init; }
}
