using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using FluentAssertions;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class OuraTokenServiceTests
{
    private readonly OuraOAuthOptions _options;
    private readonly Mock<HttpMessageHandler> _handler;
    private readonly Mock<IHttpClientFactory> _factory;
    private readonly Mock<IOuraTokenStore> _store;

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
        _store = new Mock<IOuraTokenStore>();

        var client = new HttpClient(_handler.Object)
        {
            BaseAddress = new Uri("https://api.ouraring.com")
        };
        _factory.Setup(f => f.CreateClient("OuraAuth")).Returns(client);
    }

    private OuraTokenService CreateService() =>
        new(_factory.Object, Options.Create(_options), _store.Object);

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

    #region GetAccessTokenAsync

    [Fact]
    public async Task GetAccessTokenAsync_WithValidStoredToken_ReturnsAccessToken()
    {
        var stored = new StoredTokenData("valid-access-token", "refresh-token",
            DateTimeOffset.UtcNow.AddHours(1));
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var service = CreateService();

        var result = await service.GetAccessTokenAsync();

        result.Should().Be("valid-access-token");
    }

    [Fact]
    public async Task GetAccessTokenAsync_WithExpiredToken_RefreshesAndSaves()
    {
        var expired = new StoredTokenData("old-access", "old-refresh",
            DateTimeOffset.UtcNow.AddSeconds(-10));
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expired);

        const string refreshResponse = """
            {
                "access_token": "new-access-token",
                "refresh_token": "new-refresh-token",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, refreshResponse);

        var service = CreateService();

        var result = await service.GetAccessTokenAsync();

        result.Should().Be("new-access-token");
        _store.Verify(s => s.SaveAsync(
            It.Is<StoredTokenData>(t =>
                t.AccessToken == "new-access-token" &&
                t.RefreshToken == "new-refresh-token"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccessTokenAsync_NoStoredTokens_ThrowsInvalidOperation()
    {
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredTokenData?)null);

        var service = CreateService();

        var act = () => service.GetAccessTokenAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No Oura tokens found*");
    }

    #endregion

    #region ExchangeAndStoreAsync

    [Fact]
    public async Task ExchangeAndStoreAsync_ValidCode_SavesTokens()
    {
        const string tokenResponse = """
            {
                "access_token": "new-access",
                "refresh_token": "new-refresh",
                "expires_in": 86400,
                "token_type": "bearer"
            }
            """;
        SetupTokenResponse(HttpStatusCode.OK, tokenResponse);

        var service = CreateService();

        await service.ExchangeAndStoreAsync("valid-auth-code");

        _store.Verify(s => s.SaveAsync(
            It.Is<StoredTokenData>(t =>
                t.AccessToken == "new-access" &&
                t.RefreshToken == "new-refresh"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExchangeAndStoreAsync_InvalidCode_ThrowsException()
    {
        const string errorResponse = """{"error":"invalid_grant"}""";
        SetupTokenResponse(HttpStatusCode.BadRequest, errorResponse);

        var service = CreateService();

        var act = () => service.ExchangeAndStoreAsync("invalid-auth-code");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region HasTokensAsync

    [Fact]
    public async Task HasTokensAsync_WithTokens_ReturnsTrue()
    {
        var stored = new StoredTokenData("access", "refresh",
            DateTimeOffset.UtcNow.AddHours(1));
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var service = CreateService();

        var result = await service.HasTokensAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasTokensAsync_NoTokens_ReturnsFalse()
    {
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredTokenData?)null);

        var service = CreateService();

        var result = await service.HasTokensAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasTokensAsync_ExpiredTokens_ReturnsFalse()
    {
        var expired = new StoredTokenData("access", "refresh",
            DateTimeOffset.UtcNow.AddSeconds(-10));
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expired);

        var service = CreateService();

        var result = await service.HasTokensAsync();

        result.Should().BeFalse();
    }

    #endregion

    #region Refresh behavior

    [Fact]
    public async Task RefreshToken_UpdatesStoredRefreshToken()
    {
        // First call: expired token triggers refresh → returns second tokens (also expired)
        // Second call: expired again → triggers another refresh → returns third tokens
        var expired = new StoredTokenData("first-access", "first-refresh",
            DateTimeOffset.UtcNow.AddSeconds(-10));

        // LoadAsync returns the initial expired token on the first call,
        // then the (still-expired) token saved after the first refresh on the second call.
        var callCount = 0;
        _store.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount switch
                {
                    1 => expired,
                    _ => new StoredTokenData("second-access", "second-refresh",
                        DateTimeOffset.UtcNow.AddSeconds(-10))
                };
            });

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
            (HttpStatusCode.OK, firstRefreshResponse),
            (HttpStatusCode.OK, secondRefreshResponse));

        var service = CreateService();

        await service.GetAccessTokenAsync();
        var finalAccessToken = await service.GetAccessTokenAsync();

        finalAccessToken.Should().Be("third-access",
            "each refresh should use the updated refresh token, not the original");

        _handler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}
