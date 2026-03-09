using OuraMcp.Auth;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// Services (interfaces will be implemented later)
// builder.Services.AddSingleton<IOuraTokenService, OuraTokenService>();
// builder.Services.AddScoped<IOuraApiClient, OuraApiClient>();

// MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
