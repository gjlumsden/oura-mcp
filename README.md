# Oura Ring MCP Server

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![MCP](https://img.shields.io/badge/MCP-1.1.0-blue?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggZmlsbD0id2hpdGUiIGQ9Ik0xMiAyQzYuNDggMiAyIDYuNDggMiAxMnM0LjQ4IDEwIDEwIDEwIDEwLTQuNDggMTAtMTBTMTcuNTIgMiAxMiAyem0wIDE4Yy00LjQyIDAtOC0zLjU4LTgtOHMzLjU4LTggOC04IDggMy41OCA4IDgtMy41OCA4LTggOHoiLz48L3N2Zz4=)](https://modelcontextprotocol.io/)
[![Oura API v2](https://img.shields.io/badge/Oura_API-v2-000000)](https://cloud.ouraring.com/v2/docs)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A **.NET 10 MCP server** that exposes [Oura Ring](https://ouraring.com/) health and wellness data as [Model Context Protocol](https://modelcontextprotocol.io/) tools for AI assistants.

## Overview

This server connects to the **Oura API v2** and surfaces ring data — sleep, activity, readiness, heart rate, and more — as MCP tools over **STDIO transport**. Authentication uses an **`az login`-style CLI flow**: run `dotnet run -- login` once to authorize with Oura in your browser, and tokens are persisted locally for automatic reuse and refresh.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An Oura API application registered at <https://cloud.ouraring.com/oauth/applications>

## Getting Started

### 1. Register an Oura OAuth Application

1. Go to <https://cloud.ouraring.com/oauth/applications> and log in with your Oura account.
2. Click **Create New Application**.
3. Fill in:
   - **Application Name**: e.g., "My MCP Server"
   - **Redirect URI**: `http://localhost:8742/callback/`
   - **Scopes**: Select all scopes your use case needs (email, personal, daily, heartrate, workout, tag, session, spo2, ring_configuration).
4. Note the **Client ID** and **Client Secret** shown after creation.

### 2. Clone and Build

```bash
git clone https://github.com/gjlumsden/oura-mcp.git
cd oura-mcp
dotnet build
dotnet test
```

### 3. Login

Run the login command to authenticate with Oura. This opens your browser to the Oura consent screen and saves tokens locally:

```powershell
$env:OURA_CLIENT_ID = "<your-client-id>"
$env:OURA_CLIENT_SECRET = "<your-client-secret>"
dotnet run --project src/OuraMcp -- login
```

Tokens are saved to `~/.oura-mcp/tokens.json`. You only need to do this once — the server refreshes tokens automatically on subsequent runs.

### 4. Configure Your MCP Client

Add the server to your MCP client config. The client launches the server process via STDIO and injects your Oura credentials as environment variables.

**VS Code (GitHub Copilot)** — add to `.vscode/mcp.json` or user settings:

```json
{
  "mcpServers": {
    "oura": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/oura-mcp/src/OuraMcp"],
      "env": {
        "OURA_CLIENT_ID": "<your-client-id>",
        "OURA_CLIENT_SECRET": "<your-client-secret>"
      }
    }
  }
}
```

**Claude Desktop** — add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "oura": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/oura-mcp/src/OuraMcp"],
      "env": {
        "OURA_CLIENT_ID": "<your-client-id>",
        "OURA_CLIENT_SECRET": "<your-client-secret>"
      }
    }
  }
}
```

Replace `path/to/oura-mcp/src/OuraMcp` with the actual path on your machine.

### 5. Start Using

Once configured, your MCP client will discover the Oura tools automatically. Try prompts like:

- *"How did I sleep last night?"*
- *"Show my readiness score for the past week"*
- *"What was my resting heart rate trend this month?"*
- *"Compare my deep sleep vs REM sleep over the last 2 weeks"*
- *"What's my VO2 max and cardiovascular age?"*
- *"How stressed was I this week?"*
- *"Show my SpO2 trends — anything concerning?"*
- *"When did I work out this week and how many calories did I burn?"*

## Combining with Other MCP Tools

The real power comes from combining Oura data with other MCP servers. For example, with [WorkIQ](https://github.com/microsoft/work-iq-mcp) (Microsoft 365 Copilot):

- *"Look at my meetings last week and correlate with my stress levels"*
- *"Which days did I have the most meetings? How did that affect my sleep?"*
- *"I had a stressful Friday — what meetings were in my calendar that day?"*
- *"Compare my readiness scores on meeting-heavy days vs focus days"*

With the [Azure MCP Server](https://github.com/Azure/azure-mcp):

- *"I'm on-call this week — show my sleep quality and readiness so I know if I'm fit for incident response"*

With GitHub MCP:

- *"Show my commit activity alongside my daily activity scores — am I coding more on high-energy days?"*

Any MCP-compatible tool can be combined — the AI assistant joins the dots across data sources automatically.

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
| `get_daily_spo2` | Retrieves daily SpO2 blood oxygen data from the Oura Ring. |
| `get_vo2_max` | Retrieves VO2 max estimates from the Oura Ring. |
| `get_cardiovascular_age` | Retrieves cardiovascular age estimates from the Oura Ring. |
| `get_tags` | Retrieves tags from the Oura Ring. |
| `get_enhanced_tags` | Retrieves enhanced tags from the Oura Ring. |

Most date-range tools accept optional `startDate` and `endDate` parameters (format: `YYYY-MM-DD`).

> **Note:** HRV (heart rate variability) data is embedded in the `get_sleep_periods` response — there is no separate HRV endpoint in the Oura API v2.

## Authentication Flow

This server uses an **`az login`-style CLI authentication** pattern, similar to the [Azure MCP Server](https://github.com/Azure/azure-mcp):

1. **One-time login:** Run `dotnet run -- login` to open your browser to the Oura OAuth consent screen.
2. **Token exchange:** After you authorize, a local callback server on `http://localhost:8742/callback/` receives the authorization code and exchanges it for access/refresh tokens.
3. **Persistent storage:** Tokens are saved to `~/.oura-mcp/tokens.json` (file permissions restricted to owner on Unix).
4. **Automatic refresh:** On startup the server loads saved tokens and refreshes them automatically when expired.

OAuth credentials (`OURA_CLIENT_ID`, `OURA_CLIENT_SECRET`) are read from environment variables — never stored in source.

## Development

```powershell
# First-time setup: authenticate with Oura
$env:OURA_CLIENT_ID = "<your-client-id>"
$env:OURA_CLIENT_SECRET = "<your-client-secret>"
dotnet run --project src/OuraMcp -- login

# Build
dotnet build

# Run tests
dotnet test

# Run the MCP server locally (STDIO)
$env:OURA_CLIENT_ID = "<your-client-id>"
$env:OURA_CLIENT_SECRET = "<your-client-secret>"
dotnet run --project src/OuraMcp
```

## Technology Stack

- **.NET 10** / C# with nullable reference types
- **ModelContextProtocol** NuGet package (MCP C# SDK) — STDIO transport
- **System.Text.Json** for serialization
- **IHttpClientFactory** for HTTP client lifecycle management

## License

MIT
