namespace OuraMcp.OuraClient;

/// <summary>
/// Configures TTL tiers for the in-memory cache used by <see cref="CachingOuraApiClient"/>.
/// Each tier reflects how frequently the underlying Oura data changes.
/// </summary>
public class CacheSettings
{
    /// <summary>Rarely changes — personal info and ring configuration (default: 60 min).</summary>
    public TimeSpan StableTtl { get; set; } = TimeSpan.FromMinutes(60);

    /// <summary>Can change infrequently — sleep, readiness, sleep periods, sleep time (default: 15 min).</summary>
    public TimeSpan ModerateTtl { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Day-anchored data that stabilises after logging — activity, stress, workouts, etc. (default: 10 min).</summary>
    public TimeSpan StandardTtl { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>High-frequency streaming data — heart rate (default: 1 min).</summary>
    public TimeSpan ShortTtl { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Builds a cache key from a method name and optional date-range parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// var key = CacheSettings.BuildCacheKey(nameof(IOuraApiClient.GetDailySleepAsync), start, end);
    /// // "GetDailySleepAsync:2026-03-01:2026-03-10"
    /// </code>
    /// </example>
    public static string BuildCacheKey(string methodName, DateOnly? startDate = null, DateOnly? endDate = null)
        => $"{methodName}:{startDate?.ToString("yyyy-MM-dd") ?? "null"}:{endDate?.ToString("yyyy-MM-dd") ?? "null"}";

    /// <summary>
    /// Builds a cache key from a method name with no parameters.
    /// </summary>
    public static string BuildCacheKey(string methodName) => methodName;
}
