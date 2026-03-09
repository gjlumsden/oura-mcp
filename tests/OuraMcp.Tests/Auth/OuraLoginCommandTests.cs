using FluentAssertions;
using Moq;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class OuraLoginCommandTests
{
    private readonly OuraOAuthOptions _options = new()
    {
        ClientId = "test-client",
        ClientSecret = "test-secret",
        AuthorizationUrl = "https://cloud.ouraring.com/oauth/authorize",
        Scopes = "email personal daily",
        RedirectUri = "http://original/callback"
    };

    private readonly Mock<IOuraTokenService> _tokenService = new();
    private readonly Mock<IOAuthCallbackListener> _callbackListener = new();
    private readonly Mock<IOuraBrowserLauncher> _browserLauncher = new();

    public OuraLoginCommandTests()
    {
        _callbackListener.Setup(l => l.CallbackUrl).Returns("http://localhost:8742/callback/");
    }

    private OuraLoginCommand CreateCommand() =>
        new(_options, _tokenService.Object, _callbackListener.Object, _browserLauncher.Object);

    #region BuildAuthorizeUrl

    [Fact]
    public void BuildAuthorizeUrl_IncludesClientId()
    {
        var command = CreateCommand();

        var url = command.BuildAuthorizeUrl();

        url.Should().Contain("client_id=test-client");
    }

    [Fact]
    public void BuildAuthorizeUrl_IncludesScopes()
    {
        var command = CreateCommand();

        var url = command.BuildAuthorizeUrl();

        url.Should().Contain("scope=email%20personal%20daily");
    }

    [Fact]
    public void BuildAuthorizeUrl_UsesCallbackListenerUrl()
    {
        var command = CreateCommand();

        var url = command.BuildAuthorizeUrl();

        url.Should().Contain("redirect_uri=http%3A%2F%2Flocalhost%3A8742%2Fcallback%2F");
    }

    #endregion

    #region RunAsync

    [Fact]
    public async Task RunAsync_OpensBrowserWithAuthorizeUrl()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth-code-123");
        var command = CreateCommand();

        await command.RunAsync();

        _browserLauncher.Verify(
            b => b.OpenUrl(It.Is<string>(url => url.Contains("client_id=test-client"))),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WaitsForCallback()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth-code-123");
        var command = CreateCommand();

        await command.RunAsync();

        _callbackListener.Verify(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ExchangesCode()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth-code-123");
        var command = CreateCommand();

        await command.RunAsync();

        _tokenService.Verify(
            t => t.ExchangeAndStoreAsync("auth-code-123", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_RestoresRedirectUri()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth-code-123");
        var command = CreateCommand();

        await command.RunAsync();

        _options.RedirectUri.Should().Be("http://original/callback");
    }

    [Fact]
    public async Task RunAsync_CallbackError_PropagatesException()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Authorization failed: access_denied"));
        var command = CreateCommand();

        var act = () => command.RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*access_denied*");
    }

    [Fact]
    public async Task RunAsync_ExchangeFails_PropagatesAndRestoresUri()
    {
        _callbackListener.Setup(l => l.WaitForCallbackAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth-code-123");
        _tokenService.Setup(t => t.ExchangeAndStoreAsync("auth-code-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Token exchange failed"));
        var command = CreateCommand();

        var act = () => command.RunAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
        _options.RedirectUri.Should().Be("http://original/callback");
    }

    #endregion
}
