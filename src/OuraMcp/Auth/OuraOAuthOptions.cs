namespace OuraMcp.Auth;

/// <summary>
/// Configuration options for the Oura Ring OAuth2 integration.
/// </summary>
public class OuraOAuthOptions
{
    /// <summary>The OAuth2 client ID registered with Oura.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The OAuth2 client secret registered with Oura.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>The redirect URI used during the OAuth2 authorization code flow.</summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>The Oura OAuth2 authorization endpoint URL.</summary>
    public string AuthorizationUrl { get; set; } = "https://cloud.ouraring.com/oauth/authorize";

    /// <summary>The Oura OAuth2 token endpoint URL.</summary>
    public string TokenUrl { get; set; } = "https://api.ouraring.com/oauth/token";

    /// <summary>Space-delimited OAuth2 scopes requested from Oura.</summary>
    public string Scopes { get; set; } = "email personal daily session heartrate tag workout spo2 ring_configuration";
}
