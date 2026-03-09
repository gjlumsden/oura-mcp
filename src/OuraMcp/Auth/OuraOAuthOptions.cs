namespace OuraMcp.Auth;

public class OuraOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = "https://cloud.ouraring.com/oauth/authorize";
    public string TokenUrl { get; set; } = "https://api.ouraring.com/oauth/token";
    public string Scopes { get; set; } = "email personal daily session heartrate tag workout spo2 ring_configuration";
}
