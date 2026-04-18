namespace OuraMcp.Auth;

/// <summary>
/// No-op implementation of <see cref="IOAuthCallbackListener"/> that never receives a callback.
/// Used in headless environments and tests where binding a real <see cref="System.Net.HttpListener"/>
/// is undesirable (selected when the <c>OURA_MCP_DISABLE_BROWSER</c> environment variable is set).
/// <see cref="WaitForCallbackAsync"/> blocks until the supplied <see cref="CancellationToken"/> is
/// signaled, at which point it throws <see cref="OperationCanceledException"/>.
/// </summary>
public sealed class NoOpOAuthCallbackListener : IOAuthCallbackListener
{
    /// <inheritdoc />
    public string CallbackUrl { get; } = "http://localhost:8742/callback/";

    /// <inheritdoc />
    public Task<string> WaitForCallbackAsync(CancellationToken ct = default)
    {
        // Block forever (until cancellation) — there is no real callback in this mode.
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled(ct));

        return tcs.Task;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No resources to release.
    }
}
