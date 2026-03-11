using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
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
    private readonly ILogger<OuraApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public OuraApiClient(IHttpClientFactory httpClientFactory, IOuraTokenService tokenService, ILogger<OuraApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ── Single-object endpoints ──────────────────────────────────────

    public Task<PersonalInfo> GetPersonalInfoAsync(CancellationToken ct = default)
        => GetSingleAsync<PersonalInfo>("v2/usercollection/personal_info", ct);

    public Task<RingConfiguration> GetRingConfigurationAsync(CancellationToken ct = default)
        => GetSingleAsync<RingConfiguration>("v2/usercollection/ring_configuration", ct);

    // ── Collection endpoints ─────────────────────────────────────────

    public Task<IReadOnlyList<DailySleep>> GetDailySleepAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailySleep>("v2/usercollection/daily_sleep", startDate, endDate, ct);

    public Task<IReadOnlyList<DailyActivity>> GetDailyActivityAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailyActivity>("v2/usercollection/daily_activity", startDate, endDate, ct);

    public Task<IReadOnlyList<DailyReadiness>> GetDailyReadinessAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailyReadiness>("v2/usercollection/daily_readiness", startDate, endDate, ct);

    public Task<IReadOnlyList<DailyStress>> GetDailyStressAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailyStress>("v2/usercollection/daily_stress", startDate, endDate, ct);

    public Task<IReadOnlyList<DailyResilience>> GetDailyResilienceAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailyResilience>("v2/usercollection/daily_resilience", startDate, endDate, ct);

    public Task<IReadOnlyList<SleepPeriod>> GetSleepPeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<SleepPeriod>("v2/usercollection/sleep", startDate, endDate, ct);

    public Task<IReadOnlyList<SleepTime>> GetSleepTimeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<SleepTime>("v2/usercollection/sleep_time", startDate, endDate, ct);

    public Task<IReadOnlyList<Workout>> GetWorkoutsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<Workout>("v2/usercollection/workout", startDate, endDate, ct);

    public Task<IReadOnlyList<Session>> GetSessionsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<Session>("v2/usercollection/session", startDate, endDate, ct);

    public Task<IReadOnlyList<HeartRate>> GetHeartRateAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<HeartRate>("v2/usercollection/heartrate", startDate, endDate, ct);

    public Task<IReadOnlyList<DailySpo2>> GetDailySpo2Async(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<DailySpo2>("v2/usercollection/daily_spo2", startDate, endDate, ct);

    public Task<IReadOnlyList<Vo2Max>> GetVo2MaxAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<Vo2Max>("v2/usercollection/vO2_max", startDate, endDate, ct);

    public Task<IReadOnlyList<CardiovascularAge>> GetCardiovascularAgeAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<CardiovascularAge>("v2/usercollection/daily_cardiovascular_age", startDate, endDate, ct);

    public Task<IReadOnlyList<Tag>> GetTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<Tag>("v2/usercollection/tag", startDate, endDate, ct);

    public Task<IReadOnlyList<EnhancedTag>> GetEnhancedTagsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<EnhancedTag>("v2/usercollection/enhanced_tag", startDate, endDate, ct);

    public Task<IReadOnlyList<RestModePeriod>> GetRestModePeriodsAsync(DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken ct = default)
        => GetCollectionAsync<RestModePeriod>("v2/usercollection/rest_mode_period", startDate, endDate, ct);

    // ── Core request helpers ─────────────────────────────────────────

    private async Task<T> GetSingleAsync<T>(string path, CancellationToken ct)
    {
        using var response = await SendWithRetryAsync(path, ct);

        var json = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (result is null)
            {
                _logger.LogError("Oura API returned null/empty response for {Path}", path);
                throw new McpException($"The Oura API returned an empty response for '{path}'.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Oura API response from {Path}", path);
            throw new McpException(
                $"Failed to parse the Oura API response for '{path}'. The API may have changed or returned invalid data.");
        }
    }

    private async Task<IReadOnlyList<T>> GetCollectionAsync<T>(
        string basePath, DateOnly? startDate, DateOnly? endDate, CancellationToken ct)
    {
        var allItems = new List<T>();
        string? nextToken = null;

        do
        {
            var url = BuildCollectionUrl(basePath, startDate, endDate, nextToken);
            using var response = await SendWithRetryAsync(url, ct);

            var json = await response.Content.ReadAsStringAsync(ct);

            OuraCollectionResponse<T>? collection;
            try
            {
                collection = JsonSerializer.Deserialize<OuraCollectionResponse<T>>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Oura API collection response from {Path}", basePath);
                throw new McpException(
                    $"Failed to parse the Oura API response for '{basePath}'. The API may have changed or returned invalid data.");
            }

            if (collection is null)
            {
                var truncatedBody = json.Length <= 512 ? json : json[..512];
                _logger.LogError(
                    "Failed to deserialize collection response from {Path}. Body (truncated): {Body}",
                    basePath, truncatedBody);
                throw new McpException(
                    $"Oura API returned an unexpected response while fetching collection data " +
                    $"from '{basePath}'. The response could not be deserialized.");
            }

            if (collection.Data is not null)
            {
                allItems.AddRange(collection.Data);
            }

            nextToken = collection.NextToken;
        }
        while (nextToken is not null);

        return allItems;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(string url, CancellationToken ct)
    {
        string accessToken;
        try
        {
            accessToken = await _tokenService.GetAccessTokenAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Failed to retrieve Oura access token for {Url}", url);
            throw new McpException(
                "Unable to authenticate with Oura. Run 'oura-mcp login' to re-authenticate.");
        }

        HttpResponseMessage response;
        try
        {
            response = await SendAuthorizedAsync(url, accessToken, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
        {
            _logger.LogError(ex, "Network error calling Oura API at {Url}", url);
            throw new McpException(
                "Failed to reach the Oura API. Check your network connection and try again.");
        }

        // 401 → re-fetch token (may trigger refresh inside token service) and retry once
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            try
            {
                accessToken = await _tokenService.GetAccessTokenAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Token refresh failed for {Url}", url);
                throw new McpException(
                    "Authentication failed after token refresh. Run 'oura-mcp login' to re-authenticate.");
            }

            try
            {
                response = await SendAuthorizedAsync(url, accessToken, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
            {
                _logger.LogError(ex, "Network error calling Oura API at {Url} after token refresh", url);
                throw new McpException(
                    "Failed to reach the Oura API. Check your network connection and try again.");
            }
        }

        // 429 → respect Retry-After header and retry once
        if (response.StatusCode == (HttpStatusCode)429)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
            response.Dispose();
            await Task.Delay(retryAfter, ct);
            try
            {
                response = await SendAuthorizedAsync(url, accessToken, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
            {
                _logger.LogError(ex, "Rate-limit retry failed for Oura API at {Url}", url);
                throw new McpException(
                    "The Oura API is rate-limiting requests and the retry also failed. Try again later.");
            }
        }

        // 502/503/504 → transient server error, retry once after a short delay
        if (response.StatusCode is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
        {
            var statusCode = (int)response.StatusCode;
            _logger.LogError("Oura API returned transient HTTP {StatusCode} for {Url}, retrying once", statusCode, url);
            response.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
            try
            {
                response = await SendAuthorizedAsync(url, accessToken, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
            {
                _logger.LogError(ex, "Transient-error retry also failed for Oura API at {Url}", url);
                throw new McpException(
                    $"The Oura API is temporarily unavailable (HTTP {statusCode}). Try again later.");
            }

            if (!response.IsSuccessStatusCode &&
                response.StatusCode is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
            {
                var retryStatusCode = (int)response.StatusCode;
                response.Dispose();
                _logger.LogError("Oura API returned HTTP {RetryStatusCode} on transient-error retry for {Url}", retryStatusCode, url);
                throw new McpException(
                    $"The Oura API is temporarily unavailable (HTTP {retryStatusCode}). Try again later.");
            }
        }

        // 403 → log status and endpoint (no body to avoid leaking user data) and throw a user-friendly MCP error
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            _logger.LogError("Oura API returned 403 Forbidden for {Url}", url);
            throw new McpException(
                $"Access denied for '{StripQueryString(url)}'. This usually means your Oura subscription has expired " +
                "or your account doesn't have access to this data type. " +
                "Check your subscription status in the Oura app.");
        }

        // Any other non-success status code
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            response.Dispose();
            _logger.LogError("Oura API returned unexpected HTTP {StatusCode} for {Url}", statusCode, url);
            throw new McpException(
                $"The Oura API returned an error (HTTP {statusCode}) for '{StripQueryString(url)}'. Try again later.");
        }

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

    /// <summary>Strips query string from a URL for use in user-facing error messages.</summary>
    private static string StripQueryString(string url)
    {
        var index = url.IndexOf('?');

        return index >= 0 ? url[..index] : url;
    }
}
