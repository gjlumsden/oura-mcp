using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OuraMcp;
using OuraMcp.Auth;
using OuraMcp.OuraClient;
using Serilog;
using Serilog.Events;

var errorLogPath = OuraMcpPaths.ErrorLogPath;

var builder = Host.CreateApplicationBuilder(args);
var debugMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OURA_DEBUG"));

// Logging: route console output to stderr so it doesn't interfere with stdio transport
builder.Logging.AddConsole(options =>
    options.LogToStandardErrorThreshold = LogLevel.Trace);

// File logging: persist errors to ~/.oura-mcp/logs/error.log for diagnostics
builder.Services.AddSerilog(config => config
    .MinimumLevel.Error()
    .WriteTo.File(
        path: errorLogPath,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}{NewLine}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 1,
        fileSizeLimitBytes: 5 * 1024 * 1024,
        shared: true));

// Validate required OAuth configuration early so users get a clear message
var clientId = builder.Configuration["OURA_CLIENT_ID"];
var clientSecret = builder.Configuration["OURA_CLIENT_SECRET"];
var missingVars = new List<string>();
if (string.IsNullOrWhiteSpace(clientId)) missingVars.Add("OURA_CLIENT_ID");
if (string.IsNullOrWhiteSpace(clientSecret)) missingVars.Add("OURA_CLIENT_SECRET");

if (missingVars.Count > 0)
{
    Console.Error.WriteLine($"Error: Required environment variable(s) not set: {string.Join(", ", missingVars)}");
    Console.Error.WriteLine("Set them as environment variables before running 'oura-mcp'.");
    Console.Error.WriteLine("See https://github.com/gjlumsden/oura-mcp#getting-started for details.");
    Environment.Exit(1);
}

// Configuration
builder.Services.Configure<OuraOAuthOptions>(opts =>
{
    opts.ClientId = clientId!;
    opts.ClientSecret = clientSecret!;
    opts.RedirectUri = builder.Configuration["OURA_REDIRECT_URI"] ?? "http://localhost:8742/callback/";
});

// DataProtection: encrypts tokens at rest using DPAPI (Windows) or key files (Linux/macOS)
builder.Services.AddDataProtection()
    .SetApplicationName("OuraMcp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

// HTTP Clients — "OuraApi" includes standard resilience (retry, circuit breaker, timeouts via Polly)
builder.Services.AddHttpClient("OuraApi", c => c.BaseAddress = new Uri("https://api.ouraring.com"))
    .AddStandardResilienceHandler(OuraResilienceDefaults.Configure);
builder.Services.AddHttpClient("OuraAuth", c => c.BaseAddress = new Uri("https://api.ouraring.com"));

// Services
builder.Services.AddSingleton<IOuraTokenStore, FileTokenStore>();
builder.Services.AddSingleton<IOuraTokenService, OuraTokenService>();

// Interactive login services. Registered as DI overrides so tests/headless scenarios can swap
// in no-op implementations. The OURA_MCP_DISABLE_BROWSER env var selects no-op variants that
// do not open a real browser or bind an HttpListener.
var disableBrowser = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OURA_MCP_DISABLE_BROWSER"));
if (disableBrowser)
{
    builder.Services.AddSingleton<IOuraBrowserLauncher, NoOpBrowserLauncher>();
    builder.Services.AddTransient<IOAuthCallbackListener, NoOpOAuthCallbackListener>();
}
else
{
    builder.Services.AddSingleton<IOuraBrowserLauncher, SystemBrowserLauncher>();
    builder.Services.AddTransient<IOAuthCallbackListener>(_ => new HttpCallbackListener());
}
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheSettings>();
builder.Services.AddScoped<OuraApiClient>();
builder.Services.AddScoped<IOuraApiClient>(sp =>
    new CachingOuraApiClient(
        sp.GetRequiredService<OuraApiClient>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<CacheSettings>()));

// MCP Server (stdio transport)
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Local helper that runs the interactive OAuth login flow using DI-resolved services.
// Used both by the explicit `login` CLI command and by the automatic first-launch flow.
// Resolving IOAuthCallbackListener and IOuraBrowserLauncher from DI allows tests and
// headless scenarios to swap in no-op implementations (see OURA_MCP_DISABLE_BROWSER).
static async Task RunLoginFlowAsync(IServiceProvider services, CancellationToken ct = default)
{
    // The interactive flow requires a real browser + HttpListener. When OURA_MCP_DISABLE_BROWSER
    // is set the no-op listener never completes, so fail fast with a clear, actionable message
    // instead of hanging forever waiting for a callback that will never arrive.
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OURA_MCP_DISABLE_BROWSER")))
    {
        throw new InvalidOperationException(
            "OURA_MCP_DISABLE_BROWSER is set, so the interactive login flow cannot run. " +
            "Provision tokens out-of-band and re-run with --no-login, " +
            "or unset OURA_MCP_DISABLE_BROWSER and run 'oura-mcp login'.");
    }

    // The DI scope owns the lifetime of the resolved listener — do NOT also wrap it in `using`,
    // which would risk double-disposing implementations that aren't idempotent.
    using var scope = services.CreateScope();
    var sp = scope.ServiceProvider;
    var options = sp.GetRequiredService<IOptions<OuraOAuthOptions>>().Value;
    var tokenService = sp.GetRequiredService<IOuraTokenService>();
    var listener = sp.GetRequiredService<IOAuthCallbackListener>();
    var browser = sp.GetRequiredService<IOuraBrowserLauncher>();
    var loginCommand = new OuraLoginCommand(options, tokenService, listener, browser);

    Console.Error.WriteLine("Opening browser for Oura authorization...");
    Console.Error.WriteLine($"If the browser doesn't open, visit: {loginCommand.BuildAuthorizeUrl()}");
    await loginCommand.RunAsync(ct);
    Console.Error.WriteLine("Login successful! Tokens saved to ~/.oura-mcp/tokens.json");
}

// Handle explicit CLI login command — build the host to resolve DI services, then run the login flow.
// This remains available for users who prefer to authenticate ahead of launching the MCP server
// (e.g., in headless/CI scenarios) but is no longer required: the server also auto-logs-in on first launch.
if (args.Contains("login"))
{
    try
    {
        using var host = builder.Build();
        await RunLoginFlowAsync(host.Services);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: Login failed. {ex.Message}");
        if (debugMode) Console.Error.WriteLine(ex.ToString());
        Console.Error.WriteLine("If the problem persists, try running 'oura-mcp login' again.");
        return 1;
    }

    return 0;
}

try
{
    using var host = builder.Build();

    // Auto-login on first launch: if no tokens are persisted, run the login flow before starting
    // the MCP stdio transport so the user is authenticated as part of the main MCP experience.
    // If tokens exist but are expired, OuraTokenService refreshes them automatically on first use.
    // Skip auto-login when --no-login is passed (useful for headless scenarios and tests).
    var skipAutoLogin = args.Contains("--no-login");
    if (!skipAutoLogin)
    {
        var tokenStore = host.Services.GetRequiredService<IOuraTokenStore>();
        var existingTokens = await tokenStore.LoadAsync();
        if (existingTokens is null)
        {
            Console.Error.WriteLine("No Oura tokens found — starting login flow...");
            try
            {
                await RunLoginFlowAsync(host.Services);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: Login failed. {ex.Message}");
                if (debugMode) Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("You can retry by re-launching the server, or run 'oura-mcp login' separately.");
                return 1;
            }
        }
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    if (debugMode) Console.Error.WriteLine(ex.ToString());
    return 1;
}

return 0;

/// <summary>
/// Partial class declaration to make the entry point accessible for integration tests.
/// </summary>
public partial class Program { }
