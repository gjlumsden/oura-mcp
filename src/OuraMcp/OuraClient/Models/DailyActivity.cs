using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura daily activity summary including calories, steps, and movement metrics.</summary>
public record DailyActivity
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("class_5_min")] public string? Class5Min { get; init; }
    [JsonPropertyName("score")] public int? Score { get; init; }
    [JsonPropertyName("active_calories")] public int? ActiveCalories { get; init; }
    [JsonPropertyName("average_met_minutes")] public double? AverageMetMinutes { get; init; }
    [JsonPropertyName("contributors")] public ActivityContributors? Contributors { get; init; }
    [JsonPropertyName("equivalent_walking_distance")] public int? EquivalentWalkingDistance { get; init; }
    [JsonPropertyName("high_activity_met_minutes")] public int? HighActivityMetMinutes { get; init; }
    [JsonPropertyName("high_activity_time")] public int? HighActivityTime { get; init; }
    [JsonPropertyName("inactivity_alerts")] public int? InactivityAlerts { get; init; }
    [JsonPropertyName("low_activity_met_minutes")] public int? LowActivityMetMinutes { get; init; }
    [JsonPropertyName("low_activity_time")] public int? LowActivityTime { get; init; }
    [JsonPropertyName("medium_activity_met_minutes")] public int? MediumActivityMetMinutes { get; init; }
    [JsonPropertyName("medium_activity_time")] public int? MediumActivityTime { get; init; }
    [JsonPropertyName("met")] public SampleModel? Met { get; init; }
    [JsonPropertyName("meters_to_target")] public int? MetersToTarget { get; init; }
    [JsonPropertyName("non_wear_time")] public int? NonWearTime { get; init; }
    [JsonPropertyName("resting_time")] public int? RestingTime { get; init; }
    [JsonPropertyName("sedentary_met_minutes")] public int? SedentaryMetMinutes { get; init; }
    [JsonPropertyName("sedentary_time")] public int? SedentaryTime { get; init; }
    [JsonPropertyName("steps")] public int? Steps { get; init; }
    [JsonPropertyName("target_calories")] public int? TargetCalories { get; init; }
    [JsonPropertyName("target_meters")] public int? TargetMeters { get; init; }
    [JsonPropertyName("total_calories")] public int? TotalCalories { get; init; }
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; init; }
}

/// <summary>Contributors to the daily activity score.</summary>
public record ActivityContributors
{
    [JsonPropertyName("meet_daily_targets")] public int? MeetDailyTargets { get; init; }
    [JsonPropertyName("move_every_hour")] public int? MoveEveryHour { get; init; }
    [JsonPropertyName("recovery_time")] public int? RecoveryTime { get; init; }
    [JsonPropertyName("stay_active")] public int? StayActive { get; init; }
    [JsonPropertyName("training_frequency")] public int? TrainingFrequency { get; init; }
    [JsonPropertyName("training_volume")] public int? TrainingVolume { get; init; }
}
