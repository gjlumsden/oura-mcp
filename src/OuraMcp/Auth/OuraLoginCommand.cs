namespace OuraMcp.Auth;

/// <summary>
/// Orchestrates the interactive OAuth login flow: opens browser, receives callback, saves tokens.
/// Dependencies are injected for testability.
/// </summary>
public class OuraLoginCommand
{
    private readonly OuraOAuthOptions _options;
    private readonly IOuraTokenService _tokenService;
    private readonly IOAuthCallbackListener _callbackListener;
    private readonly IOuraBrowserLauncher _browserLauncher;

    /// <summary>
    /// Initializes a new instance of <see cref="OuraLoginCommand"/>.
    /// </summary>
    /// <param name="options">OAuth configuration containing client ID, authorization URL, and scopes.</param>
    /// <param name="tokenService">Service used to exchange the authorization code and persist tokens.</param>
    /// <param name="callbackListener">Listener that receives the OAuth callback on localhost.</param>
    /// <param name="browserLauncher">Launches the default browser for user authorization.</param>
    public OuraLoginCommand(
        OuraOAuthOptions options,
        IOuraTokenService tokenService,
        IOAuthCallbackListener callbackListener,
        IOuraBrowserLauncher browserLauncher)
    {
        _options = options;
        _tokenService = tokenService;
        _callbackListener = callbackListener;
        _browserLauncher = browserLauncher;
    }

    /// <summary>
    /// Runs the OAuth login flow: opens browser, waits for callback, exchanges code for tokens.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(CancellationToken ct = default)
    {
        var authorizeUrl = BuildAuthorizeUrl();

        // Best-effort browser launch
        _browserLauncher.OpenUrl(authorizeUrl);

        var code = await _callbackListener.WaitForCallbackAsync(ct);

        // Temporarily override RedirectUri so the token exchange uses the local callback URL
        var originalRedirectUri = _options.RedirectUri;
        _options.RedirectUri = _callbackListener.CallbackUrl;
        try
        {
            await _tokenService.ExchangeAndStoreAsync(code, ct);
        }
        finally
        {
            _options.RedirectUri = originalRedirectUri;
        }
    }

    /// <summary>
    /// Builds the Oura authorization URL with all required query parameters.
    /// </summary>
    /// <returns>The fully-formed authorization URL.</returns>
    public string BuildAuthorizeUrl()
    {
        return $"{_options.AuthorizationUrl}" +
            $"?client_id={Uri.EscapeDataString(_options.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_callbackListener.CallbackUrl)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(_options.Scopes)}";
    }
}
