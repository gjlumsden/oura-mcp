using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using OuraMcp.Auth;

namespace OuraMcp.Tests.Auth;

public class FileTokenStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public FileTokenStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"oura-mcp-test-{Guid.NewGuid():N}");
        _dataProtectionProvider = new EphemeralDataProtectionProvider();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private FileTokenStore CreateStore(string? directory = null) =>
        new(_dataProtectionProvider, NullLogger<FileTokenStore>.Instance, directory ?? _tempDir);

    [Fact]
    public async Task LoadAsync_NoFile_ReturnsNull()
    {
        var store = CreateStore();

        var result = await store.LoadAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip()
    {
        var store = CreateStore();
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
    public async Task SaveAsync_WritesEncryptedContent()
    {
        var store = CreateStore();
        var tokens = new StoredTokenData("secret-access", "secret-refresh", DateTimeOffset.UtcNow.AddHours(1));

        await store.SaveAsync(tokens);

        var raw = await File.ReadAllTextAsync(Path.Combine(_tempDir, "tokens.json"));
        raw.Should().NotContain("secret-access", "token file should be encrypted, not plaintext");
        raw.Should().NotContain("secret-refresh", "token file should be encrypted, not plaintext");
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var store = CreateStore();
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
        var store = CreateStore(nestedDir);
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
        var store = CreateStore();

        var act = () => store.DeleteAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LoadAsync_CorruptedFile_ReturnsNull()
    {
        Directory.CreateDirectory(_tempDir);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "tokens.json"), "not-valid-encrypted-data");
        var store = CreateStore();

        var result = await store.LoadAsync();

        result.Should().BeNull("corrupted files should be handled gracefully, not throw");
    }
}
