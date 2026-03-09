namespace OuraMcp.Auth;

/// <summary>
/// Manages the lifecycle of OAuth tokens for the Oura API, including exchange, retrieval, and revocation.
/// </summary>
public interface IOuraTokenService
{
    /// <summary>
    /// Exchanges an OAuth2 authorization code for an MCP access token.
    /// </summary>
    /// <param name="authorizationCode">The authorization code from the Oura OAuth callback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An opaque MCP token that maps to the underlying Oura access/refresh tokens.</returns>
    Task<string> ExchangeCodeAsync(string authorizationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Oura access token associated with the given MCP token.
    /// </summary>
    /// <param name="mcpToken">The opaque MCP token issued during code exchange.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A valid Oura Bearer access token.</returns>
    Task<string> GetAccessTokenAsync(string mcpToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the Oura tokens associated with the given MCP token.
    /// </summary>
    /// <param name="mcpToken">The opaque MCP token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeAsync(string mcpToken, CancellationToken cancellationToken = default);
}
