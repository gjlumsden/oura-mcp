using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using OuraMcp.Auth;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.OuraClient;

/// <summary>
/// Oura API v2 client that resolves bearer tokens via <see cref="IOuraTokenService"/>
/// and handles pagination, 401 refresh-retry, 429 rate-limit back-off, and 403 errors.
/// </summary>
public class OuraApiClient : IOuraApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOuraTokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public OuraApiClient(IHttpClientFactory httpClientFactory, IOuraTokenService tokenService)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
    }

    // ── Single-object endpoints ──────────────────────────────────────

    public Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default, string mcpToken = "")
        => GetSingleAsync<PersonalInfo>("v2/usercollection/personal_info", mcpToken, ct);

    public Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default, string mcpToken = "")
        => GetSingleAsync<RingConfiguration>("v2/usercollection/ring_configuration", mcpToken, ct);

    // ── Collection endpoints ─────────────────────────────────────────

    public Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailySleep>("v2/usercollection/daily_sleep", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailyActivity>("v2/usercollection/daily_activity", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailyReadiness>("v2/usercollection/daily_readiness", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailyStress>("v2/usercollection/daily_stress", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailyResilience>("v2/usercollection/daily_resilience", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<SleepPeriod>("v2/usercollection/sleep", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<SleepTime>("v2/usercollection/sleep_time", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<Workout>("v2/usercollection/workout", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<Session>("v2/usercollection/session", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<HeartRate>("v2/usercollection/heartrate", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<HeartRateVariability>> GetHeartRateVariabilityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<HeartRateVariability>("v2/usercollection/heart_rate_variability", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<DailySpo2>("v2/usercollection/daily_spo2", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<Vo2Max>("v2/usercollection/vo2_max", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<CardiovascularAge>("v2/usercollection/daily_cardiovascular_age", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<Tag>("v2/usercollection/tag", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<EnhancedTag>("v2/usercollection/enhanced_tag", startDate, endDate, mcpToken, ct);

    public Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default, string mcpToken = "")
        => GetCollectionAsync<RestModePeriod>("v2/usercollection/rest_mode_period", startDate, endDate, mcpToken, ct);

    // ── Core request helpers ─────────────────────────────────────────

    private async Task<T> GetSingleAsync<T>(string path, string mcpToken, CancellationToken ct)
    {
        var response = await SendWithRetryAsync(path, mcpToken, ct);

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize response from {path}");
    }

    private async Task<IReadOnlyList<T>> GetCollectionAsync<T>(
        string basePath, DateOnly? startDate, DateOnly? endDate, string mcpToken, CancellationToken ct)
    {
        var allItems = new List<T>();
        string? nextToken = null;

        do
        {
            var url = BuildCollectionUrl(basePath, startDate, endDate, nextToken);
            var response = await SendWithRetryAsync(url, mcpToken, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            var collection = JsonSerializer.Deserialize<OuraCollectionResponse<T>>(json, JsonOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize collection from {basePath}");

            allItems.AddRange(collection.Data);
            nextToken = collection.NextToken;
        }
        while (nextToken is not null);

        return allItems;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(string url, string mcpToken, CancellationToken ct)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync(mcpToken, ct);
        var response = await SendAuthorizedAsync(url, accessToken, ct);

        // 401 → re-fetch token (may trigger refresh inside token service) and retry once
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            accessToken = await _tokenService.GetAccessTokenAsync(mcpToken, ct);
            response = await SendAuthorizedAsync(url, accessToken, ct);
        }

        // 429 → respect Retry-After header and retry once
        if (response.StatusCode == (HttpStatusCode)429)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
            await Task.Delay(retryAfter, ct);
            response = await SendAuthorizedAsync(url, accessToken, ct);
        }

        // 403 → throw with meaningful message
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"403 Forbidden: {body}");
        }

        response.EnsureSuccessStatusCode();
        return response;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(string url, string accessToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("OuraApi");
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await client.SendAsync(request, ct);
    }

    private static string BuildCollectionUrl(
        string path, DateOnly? startDate, DateOnly? endDate, string? nextToken)
    {
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"start_date={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParams.Add($"end_date={endDate.Value:yyyy-MM-dd}");
        if (nextToken is not null)
            queryParams.Add($"next_token={nextToken}");

        return queryParams.Count > 0 ? $"{path}?{string.Join("&", queryParams)}" : path;
    }
}
