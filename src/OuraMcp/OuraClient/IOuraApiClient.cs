using OuraMcp.OuraClient.Models;

namespace OuraMcp.OuraClient;

/// <summary>
/// Client for the Oura Ring REST API v2.
/// </summary>
public interface IOuraApiClient
{
    /// <summary>Retrieves the user's personal info from the Oura API.</summary>
    Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves ring configuration details from the Oura API.</summary>
    Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily sleep scores and summaries from the Oura API.</summary>
    Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily activity scores and step counts from the Oura API.</summary>
    Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily readiness scores from the Oura API.</summary>
    Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily stress data from the Oura API.</summary>
    Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily resilience data from the Oura API.</summary>
    Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves detailed sleep period data from the Oura API.</summary>
    Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves recommended sleep time windows from the Oura API.</summary>
    Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves workout data from the Oura API.</summary>
    Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves session data (meditation, breathing, etc.) from the Oura API.</summary>
    Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves heart rate data from the Oura API.</summary>
    Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves heart rate variability data from the Oura API.</summary>
    Task<IReadOnlyList<HeartRateVariability>> GetHeartRateVariabilityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves daily SpO2 blood oxygen data from the Oura API.</summary>
    Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves VO2 max estimates from the Oura API.</summary>
    Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves cardiovascular age estimates from the Oura API.</summary>
    Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves tags from the Oura API.</summary>
    Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves enhanced tags from the Oura API.</summary>
    Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");

    /// <summary>Retrieves rest mode period data from the Oura API.</summary>
    Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "");
}
