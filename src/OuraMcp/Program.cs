using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OuraMcp.Auth;
using OuraMcp.OuraClient;

var builder = Host.CreateApplicationBuilder(args);
var debugMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OURA_DEBUG"));

// Logging: route console output to stderr so it doesn't interfere with stdio transport
builder.Logging.AddConsole(options =>
    options.LogToStandardErrorThreshold = LogLevel.Trace);

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
    try
    {
        using var host = builder.Build();
        var options = host.Services.GetRequiredService<IOptions<OuraOAuthOptions>>().Value;
        var tokenService = host.Services.GetRequiredService<IOuraTokenService>();
        using var listener = new HttpCallbackListener();
        var browser = new SystemBrowserLauncher();
        var loginCommand = new OuraLoginCommand(options, tokenService, listener, browser);

        Console.Error.WriteLine("Opening browser for Oura authorization...");
        Console.Error.WriteLine($"If the browser doesn't open, visit: {loginCommand.BuildAuthorizeUrl()}");
        await loginCommand.RunAsync();
        Console.Error.WriteLine("Login successful! Tokens saved to ~/.oura-mcp/tokens.json");
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
