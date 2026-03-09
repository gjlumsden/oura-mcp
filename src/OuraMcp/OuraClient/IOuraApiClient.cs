using OuraMcp.OuraClient.Models;

namespace OuraMcp.OuraClient;

public interface IOuraApiClient
{
    Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default, string mcpToken = "");
    Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<HeartRateVariability>> GetHeartRateVariabilityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
    Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
}
