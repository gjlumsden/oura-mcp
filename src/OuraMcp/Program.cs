using Microsoft.AspNetCore.DataProtection;
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

// Configuration
builder.Services.Configure<OuraOAuthOptions>(opts =>
{
    opts.ClientId = builder.Configuration["OURA_CLIENT_ID"]
        ?? throw new InvalidOperationException("OURA_CLIENT_ID is required");
    opts.ClientSecret = builder.Configuration["OURA_CLIENT_SECRET"]
        ?? throw new InvalidOperationException("OURA_CLIENT_SECRET is required");
    opts.RedirectUri = builder.Configuration["OURA_REDIRECT_URI"] ?? "http://localhost:8742/callback/";
});

// DataProtection: encrypts tokens at rest using DPAPI (Windows) or key files (Linux/macOS)
builder.Services.AddDataProtection()
    .SetApplicationName("OuraMcp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

// HTTP Clients
builder.Services.AddHttpClient("OuraApi", c => c.BaseAddress = new Uri("https://api.ouraring.com"));
builder.Services.AddHttpClient("OuraAuth", c => c.BaseAddress = new Uri("https://api.ouraring.com"));

// Services
builder.Services.AddSingleton<IOuraTokenStore, FileTokenStore>();
builder.Services.AddSingleton<IOuraTokenService, OuraTokenService>();
builder.Services.AddScoped<IOuraApiClient, OuraApiClient>();

// MCP Server (stdio transport)
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Handle CLI login command — build the host to resolve DI services, then run the login flow
if (args.Contains("login"))
{
    var host = builder.Build();
    var options = host.Services.GetRequiredService<IOptions<OuraOAuthOptions>>().Value;
    var tokenService = host.Services.GetRequiredService<IOuraTokenService>();
    using var listener = new HttpCallbackListener();
    var browser = new SystemBrowserLauncher();
    var loginCommand = new OuraLoginCommand(options, tokenService, listener, browser);

    Console.Error.WriteLine("Opening browser for Oura authorization...");
    Console.Error.WriteLine($"If the browser doesn't open, visit: {loginCommand.BuildAuthorizeUrl()}");
    await loginCommand.RunAsync();
    Console.Error.WriteLine("Login successful! Tokens saved to ~/.oura-mcp/tokens.json");

    return;
}

await builder.Build().RunAsync();

/// <summary>
/// Partial class declaration to make the entry point accessible for integration tests.
/// </summary>
public partial class Program { }
