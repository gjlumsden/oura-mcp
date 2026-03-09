using System.Net;
using System.Text;

namespace OuraMcp.Auth;

/// <summary>
/// Production implementation of <see cref="IOAuthCallbackListener"/> using <see cref="HttpListener"/>
/// to receive OAuth authorization callbacks on localhost.
/// </summary>
public sealed class HttpCallbackListener : IOAuthCallbackListener, IDisposable
{
    private readonly HttpListener _listener = new();

    /// <inheritdoc />
    public string CallbackUrl { get; }

    /// <summary>
    /// Initializes a new listener bound to the specified localhost port.
    /// </summary>
    /// <param name="port">The port to listen on (default 8742).</param>
    public HttpCallbackListener(int port = 8742)
    {
        CallbackUrl = $"http://localhost:{port}/callback/";
    }

    /// <inheritdoc />
    public async Task<string> WaitForCallbackAsync(CancellationToken ct = default)
    {
        _listener.Prefixes.Add(CallbackUrl);
        _listener.Start();

        try
        {
            var context = await _listener.GetContextAsync().WaitAsync(ct);
            var error = context.Request.QueryString["error"];
            var code = context.Request.QueryString["code"];

            if (!string.IsNullOrEmpty(error))
            {
                await RespondAsync(context, "Authorization failed. You can close this tab.");

                throw new InvalidOperationException($"Authorization failed: {error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                await RespondAsync(context, "No authorization code received. You can close this tab.");

                throw new InvalidOperationException("No authorization code received.");
            }

            await RespondAsync(context, "Authorization successful! You can close this tab.");

            return code;
        }
        finally
        {
            _listener.Stop();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ((IDisposable)_listener).Dispose();
    }

    /// <summary>
    /// Sends an HTML response to the browser after the OAuth callback.
    /// </summary>
    private static async Task RespondAsync(HttpListenerContext context, string message)
    {
        var html = $"<html><body><h2>{message}</h2></body></html>";
        var buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
    }
}
