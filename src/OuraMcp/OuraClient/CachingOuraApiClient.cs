using Microsoft.Extensions.Caching.Memory;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.OuraClient;

/// <summary>
/// Decorator that wraps an <see cref="IOuraApiClient"/> with in-memory caching.
/// Each endpoint is cached with a TTL from <see cref="CacheSettings"/> based on
/// how frequently the underlying Oura data changes.
/// </summary>
/// <remarks>
/// Cache is in-memory only and cleared on process restart.
/// Different date ranges produce different cache entries.
/// </remarks>
public class CachingOuraApiClient(IOuraApiClient inner, IMemoryCache cache, CacheSettings settings) : IOuraApiClient
{
    // ── Stable tier (60 min) — rarely changes ────────────────────────

    /// <inheritdoc />
    public Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetPersonalInfoAsync)),
            () => inner.GetPersonalInfoAsync(ct),
            settings.StableTtl);

    /// <inheritdoc />
    public Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetRingConfigurationAsync)),
            () => inner.GetRingConfigurationAsync(ct),
            settings.StableTtl);

    // ── Moderate tier (15 min) — naps can update these ───────────────

    /// <inheritdoc />
    public Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailySleepAsync), startDate, endDate),
            () => inner.GetDailySleepAsync(startDate, endDate, ct),
            settings.ModerateTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailyReadinessAsync), startDate, endDate),
            () => inner.GetDailyReadinessAsync(startDate, endDate, ct),
            settings.ModerateTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetSleepPeriodsAsync), startDate, endDate),
            () => inner.GetSleepPeriodsAsync(startDate, endDate, ct),
            settings.ModerateTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetSleepTimeAsync), startDate, endDate),
            () => inner.GetSleepTimeAsync(startDate, endDate, ct),
            settings.ModerateTtl);

    // ── Standard tier (10 min) — day-anchored, stabilises after logging ──

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailyActivityAsync), startDate, endDate),
            () => inner.GetDailyActivityAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailyStressAsync), startDate, endDate),
            () => inner.GetDailyStressAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailyResilienceAsync), startDate, endDate),
            () => inner.GetDailyResilienceAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetWorkoutsAsync), startDate, endDate),
            () => inner.GetWorkoutsAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetSessionsAsync), startDate, endDate),
            () => inner.GetSessionsAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetDailySpo2Async), startDate, endDate),
            () => inner.GetDailySpo2Async(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetVo2MaxAsync), startDate, endDate),
            () => inner.GetVo2MaxAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetCardiovascularAgeAsync), startDate, endDate),
            () => inner.GetCardiovascularAgeAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetTagsAsync), startDate, endDate),
            () => inner.GetTagsAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetEnhancedTagsAsync), startDate, endDate),
            () => inner.GetEnhancedTagsAsync(startDate, endDate, ct),
            settings.StandardTtl);

    /// <inheritdoc />
    public Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetRestModePeriodsAsync), startDate, endDate),
            () => inner.GetRestModePeriodsAsync(startDate, endDate, ct),
            settings.StandardTtl);

    // ── Short tier (1 min) — high-frequency streaming data ───────────

    /// <inheritdoc />
    public Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetOrCreateAsync(
            CacheSettings.BuildCacheKey(nameof(GetHeartRateAsync), startDate, endDate),
            () => inner.GetHeartRateAsync(startDate, endDate, ct),
            settings.ShortTtl);

    // ── Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the TTL for a given method name based on <see cref="CacheSettings"/> tiers.
    /// Exposed for testability.
    /// </summary>
    public static TimeSpan GetTtl(string methodName, CacheSettings cacheSettings) => methodName switch
    {
        nameof(GetPersonalInfoAsync) or nameof(GetRingConfigurationAsync)
            => cacheSettings.StableTtl,

        nameof(GetDailySleepAsync) or nameof(GetDailyReadinessAsync) or
        nameof(GetSleepPeriodsAsync) or nameof(GetSleepTimeAsync)
            => cacheSettings.ModerateTtl,

        nameof(GetHeartRateAsync)
            => cacheSettings.ShortTtl,

        _ => cacheSettings.StandardTtl
    };

    private async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan ttl)
    {
        if (cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            return cached;
        }

        var result = await factory();
        cache.Set(cacheKey, result, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

        return result;
    }
}
