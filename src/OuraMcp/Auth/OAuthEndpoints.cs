using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OuraMcp.Auth;

/// <summary>
/// Minimal API endpoints implementing the OAuth2 authorization server flow
/// that bridges MCP clients to the Oura Ring OAuth provider.
/// </summary>
public static class OAuthEndpoints
{
    /// <summary>
    /// Time-limited cache for pending OAuth state → redirect_uri mappings.
    /// Entries expire after 10 minutes to prevent memory leaks from abandoned auth flows.
    /// </summary>
    private static readonly MemoryCache PendingAuthorizations = new(new MemoryCacheOptions());

    /// <summary>
    /// Registers OAuth discovery, authorization, callback, and token endpoints on the app.
    /// </summary>
    public static WebApplication MapOAuthEndpoints(this WebApplication app)
    {
        app.MapGet("/.well-known/oauth-authorization-server", HandleMetadata);
        app.MapGet("/authorize", HandleAuthorize);
        app.MapGet("/callback", HandleCallback);
        app.MapPost("/token", HandleToken);
        return app;
    }

    private static IResult HandleMetadata(HttpContext context)
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        return Results.Json(new
        {
            issuer = baseUrl,
            authorization_endpoint = $"{baseUrl}/authorize",
            token_endpoint = $"{baseUrl}/token",
            response_types_supported = new[] { "code" },
            grant_types_supported = new[] { "authorization_code", "refresh_token" },
            scopes_supported = new[] { "email", "personal", "daily", "session", "heartrate", "tag", "workout", "spo2", "ring_configuration" },
            code_challenge_methods_supported = new[] { "S256" }
        });
    }

    private static IResult HandleAuthorize(
        HttpContext context,
        IOptions<OuraOAuthOptions> options)
    {
        var query = context.Request.Query;
        var clientRedirectUri = query["redirect_uri"].ToString();
        var state = query["state"].ToString();
        var scope = query["scope"].ToString();
        var codeChallenge = query["code_challenge"].ToString();
        var codeChallengeMethod = query["code_challenge_method"].ToString();

        if (string.IsNullOrEmpty(clientRedirectUri))
            return Results.BadRequest("redirect_uri is required");

        if (string.IsNullOrEmpty(state))
            return Results.BadRequest("state is required");

        // Remember the client's redirect_uri so we can redirect back in /callback
        PendingAuthorizations.Set(state, clientRedirectUri, TimeSpan.FromMinutes(10));

        var opts = options.Value;
        var serverCallbackUri = $"{context.Request.Scheme}://{context.Request.Host}/callback";

        var ouraUrl = $"{opts.AuthorizationUrl}" +
            $"?client_id={HttpUtility.UrlEncode(opts.ClientId)}" +
            $"&redirect_uri={HttpUtility.UrlEncode(serverCallbackUri)}" +
            $"&response_type=code" +
            $"&scope={HttpUtility.UrlEncode(string.IsNullOrEmpty(scope) ? opts.Scopes : scope)}" +
            $"&state={HttpUtility.UrlEncode(state)}";

        if (!string.IsNullOrEmpty(codeChallenge))
        {
            ouraUrl += $"&code_challenge={HttpUtility.UrlEncode(codeChallenge)}" +
                       $"&code_challenge_method={HttpUtility.UrlEncode(codeChallengeMethod)}";
        }

        return Results.Redirect(ouraUrl);
    }

    private static async Task<IResult> HandleCallback(
        HttpContext context,
        IOuraTokenService tokenService)
    {
        var code = context.Request.Query["code"].ToString();
        var state = context.Request.Query["state"].ToString();
        var error = context.Request.Query["error"].ToString();

        if (!string.IsNullOrEmpty(error))
        {
            var errorDesc = context.Request.Query["error_description"].ToString();
            return Results.Problem($"OAuth error from Oura: {error} - {errorDesc}");
        }

        if (string.IsNullOrEmpty(code))
            return Results.BadRequest("Missing authorization code");

        if (string.IsNullOrEmpty(state) ||
            !PendingAuthorizations.TryGetValue(state, out string? clientRedirectUri))
        {
            return Results.BadRequest("Invalid or expired state parameter");
        }

        PendingAuthorizations.Remove(state);

        if (clientRedirectUri is null)
            return Results.BadRequest("Invalid or expired state parameter");

        var mcpToken = await tokenService.ExchangeCodeAsync(code, context.RequestAborted);

        // Redirect back to the MCP client with the token and state
        var separator = clientRedirectUri.Contains('?') ? "&" : "?";
        var redirectUrl = $"{clientRedirectUri}{separator}code={HttpUtility.UrlEncode(mcpToken)}&state={HttpUtility.UrlEncode(state)}";
        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> HandleToken(
        HttpContext context,
        IOuraTokenService tokenService)
    {
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var grantType = form["grant_type"].ToString();

        if (grantType == "authorization_code")
        {
            var code = form["code"].ToString();
            if (string.IsNullOrEmpty(code))
                return Results.BadRequest(new { error = "invalid_request", error_description = "code is required" });

            var mcpToken = await tokenService.ExchangeCodeAsync(code, context.RequestAborted);
            return Results.Json(new
            {
                access_token = mcpToken,
                token_type = "Bearer",
                expires_in = 86400
            });
        }

        return Results.BadRequest(new { error = "unsupported_grant_type", error_description = $"Grant type '{grantType}' is not supported" });
    }
}
