using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace OuraMcp.Auth;

/// <summary>
/// Persists Oura OAuth tokens to <c>~/.oura-mcp/tokens.json</c> with
/// encryption-at-rest via <see cref="IDataProtector"/>. Thread-safe via
/// <see cref="SemaphoreSlim"/>. On Unix, the file is restricted to
/// owner-only (600) permissions.
/// </summary>
public sealed class FileTokenStore : IOuraTokenStore
{
    /// <summary>Data-protection purpose string used to create the protector.</summary>
    internal const string ProtectorPurpose = "OuraMcp.TokenStore";

    private static readonly string DefaultTokenDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".oura-mcp");

    private readonly string _tokenDirectory;
    private readonly string _tokenFilePath;
    private readonly IDataProtector _protector;
    private readonly ILogger<FileTokenStore> _logger;

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
    /// <param name="dataProtectionProvider">Provides encryption/decryption for token data.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public FileTokenStore(IDataProtectionProvider dataProtectionProvider, ILogger<FileTokenStore> logger)
        : this(dataProtectionProvider, logger, DefaultTokenDirectory) { }

    /// <summary>
    /// Creates a new <see cref="FileTokenStore"/> using the specified directory.
    /// Useful for testing with isolated temp directories.
    /// </summary>
    /// <param name="dataProtectionProvider">Provides encryption/decryption for token data.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="tokenDirectory">The directory where <c>tokens.json</c> is stored.</param>
    public FileTokenStore(IDataProtectionProvider dataProtectionProvider, ILogger<FileTokenStore> logger, string tokenDirectory)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
        _tokenDirectory = tokenDirectory;
        _tokenFilePath = Path.Combine(_tokenDirectory, "tokens.json");
    }

    /// <inheritdoc />
    public string FilePath => _tokenFilePath;

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

            var encrypted = await File.ReadAllTextAsync(_tokenFilePath, ct);
            var json = _protector.Unprotect(encrypted);

            return JsonSerializer.Deserialize<StoredTokenData>(json, JsonOptions);
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(ex, "Token file is corrupted or was encrypted with a different key. Please re-login with 'oura-mcp login' or 'dnx -y gjlumsden.OuraMcp -- login'");

            return null;
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
            var encrypted = _protector.Protect(json);
            await File.WriteAllTextAsync(_tokenFilePath, encrypted, ct);

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
    /// On Windows, we rely on user-profile folder ACLs plus DPAPI-backed DataProtection.
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
