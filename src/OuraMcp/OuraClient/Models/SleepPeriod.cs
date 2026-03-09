using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record SleepPeriod(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("average_breath")] double? AverageBreath,
    [property: JsonPropertyName("average_heart_rate")] double? AverageHeartRate,
    [property: JsonPropertyName("average_hrv")] int? AverageHrv,
    [property: JsonPropertyName("awake_time")] int? AwakeTime,
    [property: JsonPropertyName("bedtime_end")] DateTimeOffset? BedtimeEnd,
    [property: JsonPropertyName("bedtime_start")] DateTimeOffset? BedtimeStart,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("deep_sleep_duration")] int? DeepSleepDuration,
    [property: JsonPropertyName("efficiency")] int? Efficiency,
    [property: JsonPropertyName("heart_rate")] SampleModel? HeartRate,
    [property: JsonPropertyName("hrv")] SampleModel? Hrv,
    [property: JsonPropertyName("latency")] int? Latency,
    [property: JsonPropertyName("light_sleep_duration")] int? LightSleepDuration,
    [property: JsonPropertyName("low_battery_alert")] bool? LowBatteryAlert,
    [property: JsonPropertyName("lowest_heart_rate")] int? LowestHeartRate,
    [property: JsonPropertyName("movement_30_sec")] string? Movement30Sec,
    [property: JsonPropertyName("period")] int? Period,
    [property: JsonPropertyName("readiness")] ReadinessSummary? Readiness,
    [property: JsonPropertyName("readiness_score_delta")] int? ReadinessScoreDelta,
    [property: JsonPropertyName("rem_sleep_duration")] int? RemSleepDuration,
    [property: JsonPropertyName("restless_periods")] int? RestlessPeriods,
    [property: JsonPropertyName("sleep_phase_5_min")] string? SleepPhase5Min,
    [property: JsonPropertyName("sleep_score_delta")] int? SleepScoreDelta,
    [property: JsonPropertyName("sleep_algorithm_version")] string? SleepAlgorithmVersion,
    [property: JsonPropertyName("sleep_analysis_reason")] string? SleepAnalysisReason,
    [property: JsonPropertyName("time_in_bed")] int? TimeInBed,
    [property: JsonPropertyName("total_sleep_duration")] int? TotalSleepDuration,
    [property: JsonPropertyName("type")] string? Type
);

public record ReadinessSummary(
    [property: JsonPropertyName("contributors")] ReadinessContributors? Contributors,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("temperature_deviation")] double? TemperatureDeviation,
    [property: JsonPropertyName("temperature_trend_deviation")] double? TemperatureTrendDeviation
);
