using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace OuraMcp.Auth;

/// <summary>
/// Manages the OAuth2 token lifecycle for a single local user.
/// Tokens are persisted via <see cref="IOuraTokenStore"/> and refreshed automatically when expired.
/// </summary>
public class OuraTokenService : IOuraTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OuraOAuthOptions _options;
    private readonly IOuraTokenStore _tokenStore;

    public OuraTokenService(
        IHttpClientFactory httpClientFactory,
        IOptions<OuraOAuthOptions> options,
        IOuraTokenStore tokenStore)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _tokenStore = tokenStore;
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var stored = await _tokenStore.LoadAsync(ct)
            ?? throw new InvalidOperationException(
                "No Oura tokens found. Run 'oura-mcp login' (or 'dnx -y gjlumsden.OuraMcp -- login') to authenticate.");

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            stored = await RefreshAsync(stored, ct);
            await _tokenStore.SaveAsync(stored, ct);
        }

        return stored.AccessToken;
    }

    /// <inheritdoc />
    public async Task ExchangeAndStoreAsync(string authorizationCode, CancellationToken ct = default)
    {
        var response = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri
        }, ct);

        var tokenData = new StoredTokenData(
            response.AccessToken,
            response.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));

        await _tokenStore.SaveAsync(tokenData, ct);
    }

    /// <inheritdoc />
    public async Task<bool> HasTokensAsync(CancellationToken ct = default)
    {
        var stored = await _tokenStore.LoadAsync(ct);

        return stored is not null && stored.ExpiresAt > DateTimeOffset.UtcNow;
    }

    private async Task<StoredTokenData> RefreshAsync(StoredTokenData stored, CancellationToken ct)
    {
        var response = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = stored.RefreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        }, ct);

        return new StoredTokenData(
            response.AccessToken,
            response.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
    }

    private async Task<TokenResponse> RequestTokenAsync(
        Dictionary<string, string> formData, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("OuraAuth");
        using var content = new FormUrlEncodedContent(formData);

        var httpResponse = await client.PostAsync(_options.TokenUrl, content, ct);
        httpResponse.EnsureSuccessStatusCode();

        var json = await httpResponse.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<TokenResponse>(json)
            ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    private sealed record TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = "";

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = "";
    }
}
