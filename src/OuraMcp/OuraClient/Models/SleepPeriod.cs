using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura sleep period with detailed duration, staging, and biometric data.</summary>
public record SleepPeriod
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("average_breath")] public double? AverageBreath { get; init; }
    [JsonPropertyName("average_heart_rate")] public double? AverageHeartRate { get; init; }
    [JsonPropertyName("average_hrv")] public int? AverageHrv { get; init; }
    [JsonPropertyName("awake_time")] public int? AwakeTime { get; init; }
    [JsonPropertyName("bedtime_end")] public DateTimeOffset? BedtimeEnd { get; init; }
    [JsonPropertyName("bedtime_start")] public DateTimeOffset? BedtimeStart { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("deep_sleep_duration")] public int? DeepSleepDuration { get; init; }
    [JsonPropertyName("efficiency")] public int? Efficiency { get; init; }
    [JsonPropertyName("heart_rate")] public SampleModel? HeartRate { get; init; }
    [JsonPropertyName("hrv")] public SampleModel? Hrv { get; init; }
    [JsonPropertyName("latency")] public int? Latency { get; init; }
    [JsonPropertyName("light_sleep_duration")] public int? LightSleepDuration { get; init; }
    [JsonPropertyName("low_battery_alert")] public bool? LowBatteryAlert { get; init; }
    [JsonPropertyName("lowest_heart_rate")] public int? LowestHeartRate { get; init; }
    [JsonPropertyName("movement_30_sec")] public string? Movement30Sec { get; init; }
    [JsonPropertyName("period")] public int? Period { get; init; }
    [JsonPropertyName("readiness")] public ReadinessSummary? Readiness { get; init; }
    [JsonPropertyName("readiness_score_delta")] public int? ReadinessScoreDelta { get; init; }
    [JsonPropertyName("rem_sleep_duration")] public int? RemSleepDuration { get; init; }
    [JsonPropertyName("restless_periods")] public int? RestlessPeriods { get; init; }
    [JsonPropertyName("sleep_phase_5_min")] public string? SleepPhase5Min { get; init; }
    [JsonPropertyName("sleep_score_delta")] public int? SleepScoreDelta { get; init; }
    [JsonPropertyName("sleep_algorithm_version")] public string? SleepAlgorithmVersion { get; init; }
    [JsonPropertyName("sleep_analysis_reason")] public string? SleepAnalysisReason { get; init; }
    [JsonPropertyName("time_in_bed")] public int? TimeInBed { get; init; }
    [JsonPropertyName("total_sleep_duration")] public int? TotalSleepDuration { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
}

/// <summary>Readiness summary embedded within a sleep period.</summary>
public record ReadinessSummary
{
    [JsonPropertyName("contributors")] public ReadinessContributors? Contributors { get; init; }
    [JsonPropertyName("score")] public int? Score { get; init; }
    [JsonPropertyName("temperature_deviation")] public double? TemperatureDeviation { get; init; }
    [JsonPropertyName("temperature_trend_deviation")] public double? TemperatureTrendDeviation { get; init; }
}
