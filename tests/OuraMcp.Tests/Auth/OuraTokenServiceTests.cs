using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using FluentAssertions;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class OuraTokenServiceTests : IDisposable
{
    private readonly OuraOAuthOptions _options;
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly Mock<IHttpClientFactory> _factory;

    public OuraTokenServiceTests()
    {
        _options = new OuraOAuthOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            RedirectUri = "http://localhost/callback",
            TokenUrl = "https://api.ouraring.com/oauth/token"
        };

        _handler = new Mock<HttpMessageHandler>();
        _factory = new Mock<IHttpClientFactory>();

        var client = new HttpClient(_handler.Object)
        {
            BaseAddress = new Uri("https://api.ouraring.com")
        };
        _factory.Setup(f => f.CreateClient("OuraAuth")).Returns(client);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private OuraTokenService CreateService() =>
        new OuraTokenService(_factory.Object, Options.Create(_options));

    private void SetupTokenResponse(HttpStatusCode statusCode, string jsonResponse)
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });
    }

    private void SetupSequentialResponses(params (HttpStatusCode StatusCode, string Json)[] responses)
    {
        var sequence = _handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var (statusCode, json) in responses)
        {
            sequence.ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }

    #region ExchangeCodeAsync

    [Fact]
    public async Task ExchangeCodeAsync_ValidCode_ReturnsToken()
    {
        // Arrange
        const string tokenResponse = """
            {
                "access_token": "abc",
                "refresh_token": "def",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, tokenResponse);
        var service = CreateService();

        // Act
        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Assert
        mcpToken.Should().NotBeNullOrEmpty("a valid code exchange should return an MCP token");
    }

    [Fact]
    public async Task ExchangeCodeAsync_InvalidCode_ThrowsException()
    {
        // Arrange
        const string errorResponse = """{"error":"invalid_grant"}""";
        SetupTokenResponse(HttpStatusCode.BadRequest, errorResponse);
        var service = CreateService();

        // Act
        var act = () => service.ExchangeCodeAsync("invalid-auth-code");

        // Assert
        await act.Should().ThrowAsync<Exception>(
            "an invalid authorization code should cause an exception");
    }

    #endregion

    #region GetAccessTokenAsync

    [Fact]
    public async Task GetAccessTokenAsync_ValidMcpToken_ReturnsOuraToken()
    {
        // Arrange
        const string tokenResponse = """
            {
                "access_token": "oura-access-token-123",
                "refresh_token": "oura-refresh-token-456",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, tokenResponse);
        var service = CreateService();

        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Act
        var accessToken = await service.GetAccessTokenAsync(mcpToken);

        // Assert
        accessToken.Should().Be("oura-access-token-123",
            "the Oura access token from the exchange should be returned");
    }

    [Fact]
    public async Task GetAccessTokenAsync_InvalidMcpToken_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var bogusToken = Guid.NewGuid().ToString();

        // Act
        var act = () => service.GetAccessTokenAsync(bogusToken);

        // Assert
        await act.Should().ThrowAsync<Exception>(
            "an MCP token that was never exchanged should not resolve to an access token");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ExpiredToken_RefreshesAutomatically()
    {
        // Arrange — first response has a very short expiry (already expired)
        const string initialTokenResponse = """
            {
                "access_token": "old-access-token",
                "refresh_token": "original-refresh-token",
                "expires_in": 0,
                "token_type": "bearer"
            }
            """;
        const string refreshedTokenResponse = """
            {
                "access_token": "new-access-token",
                "refresh_token": "new-refresh-token",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;

        SetupSequentialResponses(
            (HttpStatusCode.OK, initialTokenResponse),
            (HttpStatusCode.OK, refreshedTokenResponse));

        var service = CreateService();
        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Act — token is expired, should trigger a refresh
        var accessToken = await service.GetAccessTokenAsync(mcpToken);

        // Assert
        accessToken.Should().Be("new-access-token",
            "an expired token should be refreshed automatically");

        _handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Refresh behavior

    [Fact]
    public async Task RefreshToken_UpdatesStoredRefreshToken()
    {
        // Arrange — Oura refresh tokens are single-use, so after a refresh
        // the new refresh_token must be stored for subsequent refreshes.
        const string initialTokenResponse = """
            {
                "access_token": "first-access",
                "refresh_token": "first-refresh",
                "expires_in": 0,
                "token_type": "bearer"
            }
            """;
        const string firstRefreshResponse = """
            {
                "access_token": "second-access",
                "refresh_token": "second-refresh",
                "expires_in": 0,
                "token_type": "bearer"
            }
            """;
        const string secondRefreshResponse = """
            {
                "access_token": "third-access",
                "refresh_token": "third-refresh",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;

        SetupSequentialResponses(
            (HttpStatusCode.OK, initialTokenResponse),
            (HttpStatusCode.OK, firstRefreshResponse),
            (HttpStatusCode.OK, secondRefreshResponse));

        var service = CreateService();
        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Act — first call refreshes (expired), second call refreshes again (still expired)
        await service.GetAccessTokenAsync(mcpToken);
        var finalAccessToken = await service.GetAccessTokenAsync(mcpToken);

        // Assert — proves the refresh token was updated between refreshes
        finalAccessToken.Should().Be("third-access",
            "each refresh should use the updated refresh token, not the original");

        _handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region RevokeAsync

    [Fact]
    public async Task RevokeAsync_RemovesToken()
    {
        // Arrange
        const string tokenResponse = """
            {
                "access_token": "abc",
                "refresh_token": "def",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, tokenResponse);
        var service = CreateService();
        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Act
        await service.RevokeAsync(mcpToken);

        // Assert — after revocation, the MCP token should no longer resolve
        var act = () => service.GetAccessTokenAsync(mcpToken);
        await act.Should().ThrowAsync<Exception>(
            "a revoked MCP token should no longer be valid");
    }

    #endregion

    #region Concurrency

    [Fact]
    public async Task ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        const string tokenResponse = """
            {
                "access_token": "concurrent-access-token",
                "refresh_token": "concurrent-refresh-token",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, tokenResponse);
        var service = CreateService();
        var mcpToken = await service.ExchangeCodeAsync("valid-auth-code");

        // Act — fire many concurrent reads
        const int concurrency = 50;
        var tasks = Enumerable.Range(0, concurrency)
            .Select(_ => service.GetAccessTokenAsync(mcpToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert — all should succeed and return the same access token
        results.Should().AllBe("concurrent-access-token",
            "all concurrent requests should return the same valid access token");
    }

    #endregion
}
