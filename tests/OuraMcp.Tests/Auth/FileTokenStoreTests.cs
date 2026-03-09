using FluentAssertions;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class FileTokenStoreTests : IDisposable
{
    private readonly string _tempDir;

    public FileTokenStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"oura-mcp-test-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task LoadAsync_NoFile_ReturnsNull()
    {
        var store = new FileTokenStore(_tempDir);

        var result = await store.LoadAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip()
    {
        var store = new FileTokenStore(_tempDir);
        var tokens = new StoredTokenData(
            "test-access-token",
            "test-refresh-token",
            new DateTimeOffset(2025, 7, 15, 12, 0, 0, TimeSpan.Zero));

        await store.SaveAsync(tokens);
        var loaded = await store.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.AccessToken.Should().Be("test-access-token");
        loaded.RefreshToken.Should().Be("test-refresh-token");
        loaded.ExpiresAt.Should().Be(tokens.ExpiresAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var store = new FileTokenStore(_tempDir);
        var tokens = new StoredTokenData("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        await store.SaveAsync(tokens);
        var beforeDelete = await store.LoadAsync();
        beforeDelete.Should().NotBeNull();

        await store.DeleteAsync();

        var afterDelete = await store.LoadAsync();
        afterDelete.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectory()
    {
        var nestedDir = Path.Combine(_tempDir, "nested", "deep");
        var store = new FileTokenStore(nestedDir);
        var tokens = new StoredTokenData("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        await store.SaveAsync(tokens);

        Directory.Exists(nestedDir).Should().BeTrue();
        var loaded = await store.LoadAsync();
        loaded.Should().NotBeNull();
        loaded!.AccessToken.Should().Be("access");
    }

    [Fact]
    public async Task DeleteAsync_NoFile_DoesNotThrow()
    {
        var store = new FileTokenStore(_tempDir);

        var act = () => store.DeleteAsync();

        await act.Should().NotThrowAsync();
    }
}
