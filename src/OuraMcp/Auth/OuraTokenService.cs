using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace OuraMcp.Auth;

/// <summary>
/// Manages OAuth2 token lifecycle: exchange, storage, refresh, and revocation.
/// Maps opaque MCP tokens to Oura API access/refresh tokens.
/// </summary>
public class OuraTokenService : IOuraTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OuraOAuthOptions _options;
    private readonly ConcurrentDictionary<string, StoredToken> _tokens = new();

    public OuraTokenService(IHttpClientFactory httpClientFactory, IOptions<OuraOAuthOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> ExchangeCodeAsync(string authorizationCode, CancellationToken cancellationToken = default)
    {
        var response = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri
        }, cancellationToken);

        var mcpToken = Guid.NewGuid().ToString();
        _tokens[mcpToken] = new StoredToken(
            response.AccessToken,
            response.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));

        return mcpToken;
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(string mcpToken, CancellationToken cancellationToken = default)
    {
        if (!_tokens.TryGetValue(mcpToken, out var stored))
            throw new InvalidOperationException($"Unknown MCP token: {mcpToken}");

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            var refreshed = await RefreshAsync(stored, cancellationToken);
            _tokens[mcpToken] = refreshed;
            return refreshed.AccessToken;
        }

        return stored.AccessToken;
    }

    /// <inheritdoc />
    public Task RevokeAsync(string mcpToken, CancellationToken cancellationToken = default)
    {
        _tokens.TryRemove(mcpToken, out _);
        return Task.CompletedTask;
    }

    private async Task<StoredToken> RefreshAsync(StoredToken stored, CancellationToken cancellationToken)
    {
        var response = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = stored.RefreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        }, cancellationToken);

        return new StoredToken(
            response.AccessToken,
            response.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn));
    }

    private async Task<TokenResponse> RequestTokenAsync(
        Dictionary<string, string> formData, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("OuraAuth");
        using var content = new FormUrlEncodedContent(formData);

        var httpResponse = await client.PostAsync(_options.TokenUrl, content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<TokenResponse>(json)
            ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    private sealed record StoredToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

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
