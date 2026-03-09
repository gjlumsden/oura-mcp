using OuraMcp.OuraClient.Models;

namespace OuraMcp.OuraClient;

public interface IOuraApiClient
{
    Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default);
    Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<HeartRateVariability>> GetHeartRateVariabilityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
    Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default);
}
