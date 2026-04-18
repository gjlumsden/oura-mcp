namespace OuraMcp.Auth;

/// <summary>
/// No-op implementation of <see cref="IOuraBrowserLauncher"/> that does not open a browser.
/// Used in headless environments and tests where launching a real browser is undesirable
/// (selected when the <c>OURA_MCP_DISABLE_BROWSER</c> environment variable is set).
/// </summary>
public sealed class NoOpBrowserLauncher : IOuraBrowserLauncher
{
    /// <inheritdoc />
    public void OpenUrl(string url)
    {
        // Intentionally empty — browser launch is suppressed in headless/test mode.
    }
}
