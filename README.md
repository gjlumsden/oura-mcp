# Oura Ring MCP Server

A **.NET 10 MCP server** that exposes [Oura Ring](https://ouraring.com/) health and wellness data as [Model Context Protocol](https://modelcontextprotocol.io/) tools for AI assistants.

## Overview

This server connects to the **Oura API v2** and surfaces ring data — sleep, activity, readiness, heart rate, and more — as MCP tools over **HTTP transport with SSE**. Authentication uses **OAuth2 passthrough**: the MCP host negotiates tokens with Oura on the user's behalf, so the server never stores long-lived credentials.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An Oura API application registered at <https://cloud.ouraring.com/oauth/applications>

## Setup

1. **Register an Oura OAuth app** at <https://cloud.ouraring.com/oauth/applications>.
2. Note your **Client ID** and **Client Secret**.
3. Configure your MCP client (see below).

## MCP Client Configuration

### VS Code (GitHub Copilot) / Claude Desktop

Add the server to your MCP client configuration:

```json
{
  "mcpServers": {
    "oura": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/src/OuraMcp"],
      "env": {
        "OURA_CLIENT_ID": "<your-client-id>",
        "OURA_CLIENT_SECRET": "<your-client-secret>"
      }
    }
  }
}
```

Replace `path/to/src/OuraMcp` with the actual path to the project directory on your machine.

## Available Tools

| Tool | Description |
|------|-------------|
| `get_personal_info` | Retrieves personal info from the Oura Ring account. |
| `get_ring_configuration` | Retrieves ring configuration details from the Oura Ring. |
| `get_daily_sleep` | Retrieves daily sleep scores and summaries from the Oura Ring. |
| `get_sleep_periods` | Retrieves detailed sleep period data from the Oura Ring. |
| `get_sleep_time` | Retrieves recommended sleep time windows from the Oura Ring. |
| `get_daily_activity` | Retrieves daily activity scores and step counts from the Oura Ring. |
| `get_workouts` | Retrieves workout data from the Oura Ring. |
| `get_sessions` | Retrieves session data such as meditation and breathing exercises from the Oura Ring. |
| `get_daily_readiness` | Retrieves daily readiness scores from the Oura Ring. |
| `get_daily_stress` | Retrieves daily stress data from the Oura Ring. |
| `get_daily_resilience` | Retrieves daily resilience data from the Oura Ring. |
| `get_rest_mode_periods` | Retrieves rest mode period data from the Oura Ring. |
| `get_heart_rate` | Retrieves heart rate data from the Oura Ring. |
| `get_heart_rate_variability` | Retrieves heart rate variability data from the Oura Ring. |
| `get_daily_spo2` | Retrieves daily SpO2 blood oxygen data from the Oura Ring. |
| `get_vo2_max` | Retrieves VO2 max estimates from the Oura Ring. |
| `get_cardiovascular_age` | Retrieves cardiovascular age estimates from the Oura Ring. |
| `get_tags` | Retrieves tags from the Oura Ring. |
| `get_enhanced_tags` | Retrieves enhanced tags from the Oura Ring. |

Most date-range tools accept optional `startDate` and `endDate` parameters (format: `YYYY-MM-DD`).

## Authentication Flow

This server uses **OAuth2 passthrough** — it delegates authentication to the MCP transport layer:

1. MCP client connects to the server over HTTP+SSE.
2. The server challenges with a **401** directing the client to Oura's OAuth consent screen.
3. The user authorizes access; Oura issues tokens via the authorization code grant.
4. The MCP client exchanges tokens and presents a **Bearer token** on all subsequent requests.
5. The server forwards the Bearer token to the Oura API on each upstream call.

OAuth credentials (`OURA_CLIENT_ID`, `OURA_CLIENT_SECRET`) are read from environment variables — never stored in source.

## Development

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the server locally
OURA_CLIENT_ID=<id> OURA_CLIENT_SECRET=<secret> dotnet run --project src/OuraMcp
```

## Technology Stack

- **.NET 10** / C# with nullable reference types
- **ModelContextProtocol** NuGet package (MCP C# SDK) — HTTP transport with SSE
- **ASP.NET Core** minimal API host
- **System.Text.Json** for serialization
- **IHttpClientFactory** for HTTP client lifecycle management

## License

MIT
