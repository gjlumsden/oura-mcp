using System.Runtime.InteropServices;
using System.Text.Json;

namespace OuraMcp.Auth;

/// <summary>
/// Persists Oura OAuth tokens to <c>~/.oura-mcp/tokens.json</c>.
/// Thread-safe via <see cref="SemaphoreSlim"/>. On Unix, the file is
/// restricted to owner-only (600) permissions.
/// </summary>
public sealed class FileTokenStore : IOuraTokenStore
{
    private static readonly string DefaultTokenDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".oura-mcp");

    private readonly string _tokenDirectory;
    private readonly string _tokenFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>Guards concurrent file access.</summary>
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Creates a new <see cref="FileTokenStore"/> using the default token directory
    /// (<c>~/.oura-mcp/</c>).
    /// </summary>
    public FileTokenStore() : this(DefaultTokenDirectory) { }

    /// <summary>
    /// Creates a new <see cref="FileTokenStore"/> using the specified directory.
    /// Useful for testing with isolated temp directories.
    /// </summary>
    /// <param name="tokenDirectory">The directory where <c>tokens.json</c> is stored.</param>
    public FileTokenStore(string tokenDirectory)
    {
        _tokenDirectory = tokenDirectory;
        _tokenFilePath = Path.Combine(_tokenDirectory, "tokens.json");
    }

    /// <inheritdoc />
    public async Task<StoredTokenData?> LoadAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_tokenFilePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(_tokenFilePath, ct);

            return JsonSerializer.Deserialize<StoredTokenData>(json, JsonOptions);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(StoredTokenData tokens, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(_tokenDirectory);

            var json = JsonSerializer.Serialize(tokens, JsonOptions);
            await File.WriteAllTextAsync(_tokenFilePath, json, ct);

            SetOwnerOnlyPermissions();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// On Unix, restrict the token file to owner-read/write only (chmod 600).
    /// On Windows, we rely on user-profile folder ACLs (DPAPI integration is a future improvement).
    /// </summary>
    private void SetOwnerOnlyPermissions()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(_tokenFilePath))
        {
            File.SetUnixFileMode(_tokenFilePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
