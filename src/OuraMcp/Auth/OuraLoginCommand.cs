using System.Net;

namespace OuraMcp.Auth;

/// <summary>
/// Handles the interactive OAuth login flow: opens browser, receives callback, saves tokens.
/// </summary>
public static class OuraLoginCommand
{
    /// <summary>
    /// Runs the OAuth login flow: opens browser to Oura consent, listens on localhost for callback,
    /// exchanges the authorization code for tokens, and saves them to the token store.
    /// </summary>
    /// <param name="options">OAuth configuration containing client ID, authorization URL, and scopes.</param>
    /// <param name="tokenService">Service used to exchange the authorization code and persist tokens.</param>
    public static async Task RunAsync(OuraOAuthOptions options, IOuraTokenService tokenService)
    {
        const int port = 8742;
        var callbackUrl = $"http://localhost:{port}/callback/";

        using var listener = new HttpListener();
        listener.Prefixes.Add(callbackUrl);
        listener.Start();

        var authorizeUrl = $"{options.AuthorizationUrl}" +
            $"?client_id={Uri.EscapeDataString(options.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(options.Scopes)}" +
            $"&state={Guid.NewGuid()}";

        Console.Error.WriteLine("Opening browser for Oura authorization...");
        Console.Error.WriteLine($"If the browser doesn't open, visit: {authorizeUrl}");
        OpenBrowser(authorizeUrl);

        Console.Error.WriteLine("Waiting for authorization...");
        var context = await listener.GetContextAsync();
        var code = context.Request.QueryString["code"];
        var error = context.Request.QueryString["error"];

        if (!string.IsNullOrEmpty(error))
        {
            await RespondAsync(context, "Authorization failed. You can close this tab.");
            listener.Stop();
            Console.Error.WriteLine($"Authorization failed: {error}");

            return;
        }

        if (string.IsNullOrEmpty(code))
        {
            await RespondAsync(context, "No authorization code received. You can close this tab.");
            listener.Stop();
            Console.Error.WriteLine("No authorization code received.");

            return;
        }

        await RespondAsync(context, "Authorization successful! You can close this tab.");
        listener.Stop();

        // Temporarily override RedirectUri so the token exchange uses the local callback URL
        var originalRedirectUri = options.RedirectUri;
        options.RedirectUri = callbackUrl;
        try
        {
            await tokenService.ExchangeAndStoreAsync(code);
            Console.Error.WriteLine("Login successful! Tokens saved to ~/.oura-mcp/tokens.json");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Token exchange failed: {ex.Message}");
        }
        finally
        {
            options.RedirectUri = originalRedirectUri;
        }
    }

    /// <summary>
    /// Sends an HTML response to the browser after the OAuth callback.
    /// </summary>
    private static async Task RespondAsync(HttpListenerContext context, string message)
    {
        var html = $"<html><body><h2>{message}</h2></body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(html);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
    }

    /// <summary>
    /// Opens the default browser to the specified URL. Best-effort; failures are silently ignored.
    /// </summary>
    private static void OpenBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", url);
            }
            else
            {
                System.Diagnostics.Process.Start("xdg-open", url);
            }
        }
        catch
        {
            // Browser opening is best-effort
        }
    }
}
