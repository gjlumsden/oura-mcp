using OuraMcp.Auth;
using OuraMcp.OuraClient;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Logging: route console output to stderr so it doesn't interfere with MCP transport
builder.Logging.AddConsole(options =>
    options.LogToStandardErrorThreshold = LogLevel.Trace);

// Configuration
builder.Services.Configure<OuraOAuthOptions>(opts =>
{
    opts.ClientId = builder.Configuration["OURA_CLIENT_ID"]
        ?? throw new InvalidOperationException("OURA_CLIENT_ID is required");
    opts.ClientSecret = builder.Configuration["OURA_CLIENT_SECRET"]
        ?? throw new InvalidOperationException("OURA_CLIENT_SECRET is required");
    opts.RedirectUri = builder.Configuration["OURA_REDIRECT_URI"] ?? "http://localhost:5000/callback";
});

// HTTP Clients
builder.Services.AddHttpClient("OuraApi", c => c.BaseAddress = new Uri("https://api.ouraring.com"));
builder.Services.AddHttpClient("OuraAuth", c => c.BaseAddress = new Uri("https://api.ouraring.com"));

// Services
builder.Services.AddSingleton<IOuraTokenService, OuraTokenService>();
builder.Services.AddScoped<IOuraApiClient, OuraApiClient>();

// MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapOAuthEndpoints();
app.MapMcp("/mcp");

app.Run();

/// <summary>
/// Partial class declaration to make the entry point accessible
/// for integration tests using <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public partial class Program { }
