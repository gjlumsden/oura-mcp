using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

/// <summary>
/// Integration tests for the OAuth2 endpoints defined in <see cref="OAuthEndpoints"/>.
/// Uses <see cref="WebApplicationFactory{TEntryPoint}"/> to spin up an in-memory test server
/// with a mocked <see cref="IOuraTokenService"/> so no real Oura API calls are made.
/// </summary>
public class OAuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Mock<IOuraTokenService> _mockTokenService;

    public OAuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _mockTokenService = new Mock<IOuraTokenService>();
        _mockTokenService
            .Setup(t => t.ExchangeCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("mock-mcp-token");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOuraTokenService));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton(_mockTokenService.Object);
            });
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["OURA_CLIENT_ID"] = "test-client-id",
                    ["OURA_CLIENT_SECRET"] = "test-client-secret"
                });
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Metadata endpoint

    [Fact]
    public async Task Metadata_ReturnsCorrectJson()
    {
        var response = await _client.GetAsync("/.well-known/oauth-authorization-server");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var json = doc.RootElement;

        json.GetProperty("issuer").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("authorization_endpoint").GetString().Should().EndWith("/authorize");
        json.GetProperty("token_endpoint").GetString().Should().EndWith("/token");

        json.GetProperty("response_types_supported").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("code");

        json.GetProperty("grant_types_supported").EnumerateArray()
            .Select(e => e.GetString())
            .Should().Contain("authorization_code")
            .And.Contain("refresh_token");

        json.GetProperty("scopes_supported").EnumerateArray()
            .Should().NotBeEmpty();

        json.GetProperty("code_challenge_methods_supported").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("S256");
    }

    #endregion

    #region Authorize endpoint

    [Fact]
    public async Task Authorize_WithValidParams_RedirectsToOura()
    {
        var response = await _client.GetAsync(
            "/authorize?redirect_uri=http://localhost/cb&state=abc123&scope=daily");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var location = response.Headers.Location!;
        location.Host.Should().Be("cloud.ouraring.com");
        location.AbsolutePath.Should().Be("/oauth/authorize");

        var query = System.Web.HttpUtility.ParseQueryString(location.Query);
        query["client_id"].Should().Be("test-client-id");
        query["response_type"].Should().Be("code");
        query["scope"].Should().Be("daily");
        query["state"].Should().Be("abc123");
        query["redirect_uri"].Should().Contain("/callback");
    }

    [Fact]
    public async Task Authorize_MissingRedirectUri_Returns400()
    {
        var response = await _client.GetAsync("/authorize?state=abc");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Authorize_MissingState_Returns400()
    {
        var response = await _client.GetAsync("/authorize?redirect_uri=http://localhost/cb");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Authorize_PassesPkceParams()
    {
        var response = await _client.GetAsync(
            "/authorize?redirect_uri=http://localhost/cb&state=pkce-state" +
            "&code_challenge=test-challenge&code_challenge_method=S256");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var location = response.Headers.Location!;
        var query = System.Web.HttpUtility.ParseQueryString(location.Query);
        query["code_challenge"].Should().Be("test-challenge");
        query["code_challenge_method"].Should().Be("S256");
    }

    #endregion

    #region Callback endpoint

    [Fact]
    public async Task Callback_WithError_ReturnsProblem()
    {
        var response = await _client.GetAsync(
            "/callback?error=access_denied&state=err-state");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("access_denied");
    }

    [Fact]
    public async Task Callback_MissingCode_Returns400()
    {
        var response = await _client.GetAsync("/callback?state=abc");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Callback_InvalidState_Returns400()
    {
        var response = await _client.GetAsync("/callback?code=abc&state=unknown");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Callback_ValidStateAndCode_RedirectsToClientWithToken()
    {
        // Register the state via /authorize first so it's in the pending cache
        await _client.GetAsync(
            "/authorize?redirect_uri=http://localhost/cb&state=valid-cb-state&scope=daily");

        var response = await _client.GetAsync(
            "/callback?code=test-auth-code&state=valid-cb-state");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var location = response.Headers.Location!;
        location.ToString().Should().StartWith("http://localhost/cb");

        var query = System.Web.HttpUtility.ParseQueryString(location.Query);
        query["code"].Should().Be("mock-mcp-token");
        query["state"].Should().Be("valid-cb-state");
    }

    #endregion

    #region Token endpoint

    [Fact]
    public async Task Token_AuthorizationCode_MissingCode_Returns400()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code"
        });

        var response = await _client.PostAsync("/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("error").GetString().Should().Be("invalid_request");
    }

    [Fact]
    public async Task Token_UnsupportedGrantType_Returns400()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        var response = await _client.PostAsync("/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("error").GetString()
            .Should().Be("unsupported_grant_type");
    }

    [Fact]
    public async Task Token_AuthorizationCode_ValidCode_ReturnsAccessToken()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = "test-auth-code"
        });

        var response = await _client.PostAsync("/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var json = doc.RootElement;
        json.GetProperty("access_token").GetString().Should().Be("mock-mcp-token");
        json.GetProperty("token_type").GetString().Should().Be("Bearer");
        json.GetProperty("expires_in").GetInt32().Should().Be(86400);
    }

    #endregion
}
