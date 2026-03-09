namespace OuraMcp.Auth;

/// <summary>
/// Abstracts browser launching for OAuth authorization.
/// </summary>
public interface IOuraBrowserLauncher
{
    /// <summary>
    /// Open the default browser to the specified URL.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenUrl(string url);
}
