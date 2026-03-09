using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyReadiness(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("contributors")] ReadinessContributors? Contributors,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("temperature_deviation")] double? TemperatureDeviation,
    [property: JsonPropertyName("temperature_trend_deviation")] double? TemperatureTrendDeviation,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);

public record ReadinessContributors(
    [property: JsonPropertyName("activity_balance")] int? ActivityBalance,
    [property: JsonPropertyName("body_temperature")] int? BodyTemperature,
    [property: JsonPropertyName("hrv_balance")] int? HrvBalance,
    [property: JsonPropertyName("previous_day_activity")] int? PreviousDayActivity,
    [property: JsonPropertyName("previous_night")] int? PreviousNight,
    [property: JsonPropertyName("recovery_index")] int? RecoveryIndex,
    [property: JsonPropertyName("resting_heart_rate")] int? RestingHeartRate,
    [property: JsonPropertyName("sleep_balance")] int? SleepBalance,
    [property: JsonPropertyName("sleep_regularity")] int? SleepRegularity
);
