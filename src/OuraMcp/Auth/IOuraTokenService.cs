namespace OuraMcp.Auth;

public interface IOuraTokenService
{
    Task<string> ExchangeCodeAsync(string authorizationCode, CancellationToken cancellationToken = default);
    Task<string> GetAccessTokenAsync(string mcpToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string mcpToken, CancellationToken cancellationToken = default);
}
