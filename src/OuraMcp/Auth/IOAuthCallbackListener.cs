namespace OuraMcp.Auth;

/// <summary>
/// Abstracts the localhost HTTP listener that receives OAuth callbacks.
/// Implementations may hold OS resources (e.g., a bound port), so the interface
/// extends <see cref="IDisposable"/> for deterministic cleanup. No-op
/// implementations may leave <see cref="IDisposable.Dispose"/> empty.
/// </summary>
public interface IOAuthCallbackListener : IDisposable
{
    /// <summary>The callback URL the listener is bound to.</summary>
    string CallbackUrl { get; }

    /// <summary>
    /// Start listening for a single OAuth callback. Returns the authorization code,
    /// or throws on error.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The authorization code from the callback.</returns>
    Task<string> WaitForCallbackAsync(CancellationToken ct = default);
}
