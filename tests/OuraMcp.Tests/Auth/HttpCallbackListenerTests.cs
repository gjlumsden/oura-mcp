using FluentAssertions;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class HttpCallbackListenerTests
{
    [Fact]
    public void CallbackUrl_FormattedCorrectly()
    {
        using var listener = new HttpCallbackListener(19999);

        listener.CallbackUrl.Should().Be("http://localhost:19999/callback/");
    }

    [Fact]
    public void CallbackUrl_UsesDefaultPort()
    {
        using var listener = new HttpCallbackListener();

        listener.CallbackUrl.Should().Be("http://localhost:8742/callback/");
    }

    [Fact]
    public async Task WaitForCallbackAsync_WithCode_ReturnsCode()
    {
        using var listener = new HttpCallbackListener(19998);
        var waitTask = listener.WaitForCallbackAsync();

        using var client = new HttpClient();
        await client.GetAsync("http://localhost:19998/callback/?code=test-auth-code&state=xyz");

        var code = await waitTask;
        code.Should().Be("test-auth-code");
    }

    [Fact]
    public async Task WaitForCallbackAsync_WithError_ThrowsInvalidOperation()
    {
        using var listener = new HttpCallbackListener(19997);
        var waitTask = listener.WaitForCallbackAsync();

        using var client = new HttpClient();
        await client.GetAsync("http://localhost:19997/callback/?error=access_denied");

        var act = () => waitTask;
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*access_denied*");
    }

    [Fact]
    public async Task WaitForCallbackAsync_NoCode_ThrowsInvalidOperation()
    {
        using var listener = new HttpCallbackListener(19996);
        var waitTask = listener.WaitForCallbackAsync();

        using var client = new HttpClient();
        await client.GetAsync("http://localhost:19996/callback/");

        var act = () => waitTask;
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No authorization code*");
    }

    [Fact]
    public async Task WaitForCallbackAsync_Cancellation_ThrowsTaskCanceled()
    {
        using var listener = new HttpCallbackListener(19995);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var act = () => listener.WaitForCallbackAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
