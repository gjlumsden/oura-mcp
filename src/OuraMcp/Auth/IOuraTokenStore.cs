namespace OuraMcp.Auth;

/// <summary>Persists Oura OAuth tokens to disk.</summary>
public interface IOuraTokenStore
{
    /// <summary>Load stored tokens, or <c>null</c> if none exist.</summary>
    Task<StoredTokenData?> LoadAsync(CancellationToken ct = default);

    /// <summary>Save tokens to disk.</summary>
    Task SaveAsync(StoredTokenData tokens, CancellationToken ct = default);

    /// <summary>Delete stored tokens.</summary>
    Task DeleteAsync(CancellationToken ct = default);
}

/// <summary>Token data persisted to disk.</summary>
public record StoredTokenData(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
