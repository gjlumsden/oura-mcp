using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record DailyActivity(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("class_5_min")] string? Class5Min,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("active_calories")] int? ActiveCalories,
    [property: JsonPropertyName("average_met_minutes")] double? AverageMetMinutes,
    [property: JsonPropertyName("contributors")] ActivityContributors? Contributors,
    [property: JsonPropertyName("equivalent_walking_distance")] int? EquivalentWalkingDistance,
    [property: JsonPropertyName("high_activity_met_minutes")] int? HighActivityMetMinutes,
    [property: JsonPropertyName("high_activity_time")] int? HighActivityTime,
    [property: JsonPropertyName("inactivity_alerts")] int? InactivityAlerts,
    [property: JsonPropertyName("low_activity_met_minutes")] int? LowActivityMetMinutes,
    [property: JsonPropertyName("low_activity_time")] int? LowActivityTime,
    [property: JsonPropertyName("medium_activity_met_minutes")] int? MediumActivityMetMinutes,
    [property: JsonPropertyName("medium_activity_time")] int? MediumActivityTime,
    [property: JsonPropertyName("met")] SampleModel? Met,
    [property: JsonPropertyName("meters_to_target")] int? MetersToTarget,
    [property: JsonPropertyName("non_wear_time")] int? NonWearTime,
    [property: JsonPropertyName("resting_time")] int? RestingTime,
    [property: JsonPropertyName("sedentary_met_minutes")] int? SedentaryMetMinutes,
    [property: JsonPropertyName("sedentary_time")] int? SedentaryTime,
    [property: JsonPropertyName("steps")] int? Steps,
    [property: JsonPropertyName("target_calories")] int? TargetCalories,
    [property: JsonPropertyName("target_meters")] int? TargetMeters,
    [property: JsonPropertyName("total_calories")] int? TotalCalories,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp
);

public record ActivityContributors(
    [property: JsonPropertyName("meet_daily_targets")] int? MeetDailyTargets,
    [property: JsonPropertyName("move_every_hour")] int? MoveEveryHour,
    [property: JsonPropertyName("recovery_time")] int? RecoveryTime,
    [property: JsonPropertyName("stay_active")] int? StayActive,
    [property: JsonPropertyName("training_frequency")] int? TrainingFrequency,
    [property: JsonPropertyName("training_volume")] int? TrainingVolume
);
