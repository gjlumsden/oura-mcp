using System.Diagnostics;

namespace OuraMcp.Auth;

/// <summary>
/// Production implementation of <see cref="IOuraBrowserLauncher"/> that opens
/// the default system browser. Failures are silently ignored (best-effort).
/// </summary>
public sealed class SystemBrowserLauncher : IOuraBrowserLauncher
{
    /// <inheritdoc />
    public void OpenUrl(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", url);
            }
            else
            {
                Process.Start("xdg-open", url);
            }
        }
        catch
        {
            // Browser opening is best-effort
        }
    }
}
