namespace OuraMcp.Auth;

/// <summary>
/// Manages the lifecycle of Oura OAuth tokens for a single local user,
/// including exchange, retrieval with auto-refresh, and persistence.
/// </summary>
public interface IOuraTokenService
{
    /// <summary>
    /// Gets a valid Oura access token, refreshing automatically if expired.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A valid Oura Bearer access token.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Exchanges an authorization code for tokens and persists them to disk.
    /// </summary>
    /// <param name="authorizationCode">The authorization code from the Oura OAuth callback.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExchangeAndStoreAsync(string authorizationCode, CancellationToken ct = default);

    /// <summary>
    /// Checks whether valid (non-expired) tokens exist in the store.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if persisted tokens exist and have not expired.</returns>
    Task<bool> HasTokensAsync(CancellationToken ct = default);
}
