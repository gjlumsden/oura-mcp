# Oura Ring MCP Server

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![MCP](https://img.shields.io/badge/MCP-1.1.0-blue?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggZmlsbD0id2hpdGUiIGQ9Ik0xMiAyQzYuNDggMiAyIDYuNDggMiAxMnM0LjQ4IDEwIDEwIDEwIDEwLTQuNDggMTAtMTBTMTcuNTIgMiAxMiAyem0wIDE4Yy00LjQyIDAtOC0zLjU4LTgtOHMzLjU4LTggOC04IDggMy41OCA4IDgtMy41OCA4LTggOHoiLz48L3N2Zz4=)](https://modelcontextprotocol.io/)
[![Oura API v2](https://img.shields.io/badge/Oura_API-v2-000000)](https://developer.ouraring.com/)
[![NuGet](https://img.shields.io/nuget/v/gjlumsden.OuraMcp?logo=nuget)](https://www.nuget.org/packages/gjlumsden.OuraMcp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A **.NET 10 MCP server** that exposes [Oura Ring](https://ouraring.com/) health and wellness data as [Model Context Protocol](https://modelcontextprotocol.io/) tools for AI assistants.

> **Disclaimer:** This is an independently developed open source project. It is not affiliated with, endorsed by, or sponsored by Ōura Health Oy. "Oura" and "Oura Ring" are trademarks of Ōura Health Oy. This project uses the [Oura API v2](https://developer.ouraring.com/) but is not maintained or supported by Oura. For official Oura support, visit [ouraring.com](https://ouraring.com/).

## Overview

This server connects to the **Oura API v2** and surfaces ring data — sleep, activity, readiness, heart rate, and more — as MCP tools over **STDIO transport**. Authentication is **automatic on first launch**: the server opens your browser for Oura authorization the first time it starts, and tokens are persisted locally for silent reuse and refresh. An explicit `oura-mcp login` command is also available for headless or ahead-of-time setup.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An Oura API application registered at <https://developer.ouraring.com/> (redirect URI: `http://localhost:8742/callback/`)

### 1. Install

```powershell
dotnet tool install -g gjlumsden.OuraMcp
```

### No-Install Option

Or run without installing (requires .NET 10):

```powershell
dnx -y gjlumsden.OuraMcp
```

### 2. Configure Your MCP Client

Add the server to your MCP client config. The client launches the server process via STDIO and injects your Oura credentials as environment variables. **The first time the server starts, it will automatically open your browser to authorize with Oura** — no separate login step is required. Tokens are then saved to `~/.oura-mcp/tokens.json` and reused/refreshed on subsequent launches.

**VS Code (GitHub Copilot)** — add to `.vscode/mcp.json` or user settings:

```json
{
  "mcpServers": {
    "oura": {
      "command": "oura-mcp",
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
      "command": "oura-mcp",
      "env": {
        "OURA_CLIENT_ID": "<your-client-id>",
        "OURA_CLIENT_SECRET": "<your-client-secret>"
      }
    }
  }
}
```

<details>
<summary><strong>Using the no-install (dnx) option?</strong></summary>

Replace the server entry with:

```json
{
  "mcpServers": {
    "oura": {
      "command": "dnx",
      "args": ["-y", "gjlumsden.OuraMcp"],
      "env": {
        "OURA_CLIENT_ID": "<your-client-id>",
        "OURA_CLIENT_SECRET": "<your-client-secret>"
      }
    }
  }
}
```

</details>

<details>
<summary><strong>Prefer to log in ahead of time (headless / CI)?</strong></summary>

You can authenticate explicitly before wiring the server into an MCP client by running the `login` subcommand:

```powershell
$env:OURA_CLIENT_ID = "<your-client-id>"
$env:OURA_CLIENT_SECRET = "<your-client-secret>"
oura-mcp login              # or: dnx -y gjlumsden.OuraMcp -- login
```

Tokens are saved to `~/.oura-mcp/tokens.json`; subsequent server launches will skip the browser prompt. You can also pass `--no-login` to `oura-mcp` to suppress the auto-login prompt on startup (useful if tokens are provisioned out-of-band).

</details>

### 3. Start Using

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

This server uses an **`az login`-style OAuth authentication** pattern, similar to the [Azure MCP Server](https://github.com/Azure/azure-mcp), integrated into the main MCP experience:

1. **Automatic first launch:** When the server starts and no tokens are present, it opens your browser to the Oura OAuth consent screen as part of its normal startup — no separate command is required. Pass `--no-login` to suppress this behavior if you provision tokens out-of-band.
2. **Explicit login (optional):** You can still run `oura-mcp login` (or `dnx -y gjlumsden.OuraMcp -- login` for the no-install option, or `dotnet run --project src/OuraMcp -- login` from source) to authenticate ahead of time (e.g., headless setup, CI).
3. **Token exchange:** After you authorize, a local callback server on `http://localhost:8742/callback/` receives the authorization code and exchanges it for access/refresh tokens.
4. **Persistent storage:** Tokens are saved to `~/.oura-mcp/tokens.json` (file permissions restricted to owner on Unix).
5. **Automatic refresh:** On subsequent startups the server loads saved tokens and refreshes them automatically when expired.

OAuth credentials (`OURA_CLIENT_ID`, `OURA_CLIENT_SECRET`) are read from environment variables — never stored in source.

## Development

To contribute or run from source:

```bash
git clone https://github.com/gjlumsden/oura-mcp.git
cd oura-mcp
dotnet build
dotnet test
```

```powershell
# Set OAuth credentials
$env:OURA_CLIENT_ID = "<your-client-id>"
$env:OURA_CLIENT_SECRET = "<your-client-secret>"

# Run the MCP server locally (STDIO) — opens the browser automatically on first launch
dotnet run --project src/OuraMcp

# Or authenticate explicitly first (optional; useful for headless scenarios)
dotnet run --project src/OuraMcp -- login
```

## Caching

The server includes **in-memory caching** to reduce redundant Oura API calls within a session. When an AI assistant calls the same tool with the same parameters, cached data is returned instead of hitting the API again.

Cache is **in-memory only** — it's cleared automatically when the server process restarts. Different date ranges are cached independently.

| TTL | Endpoints | Rationale |
|-----|-----------|-----------|
| **60 min** | Personal info, ring configuration | Rarely changes |
| **15 min** | Sleep, readiness, sleep periods, sleep time | Can change (e.g., naps) but not often |
| **10 min** | Activity, stress, resilience, workouts, sessions, SpO2, VO2 max, cardiovascular age, tags, rest mode | Day-anchored data that stabilises after logging |
| **1 min** | Heart rate | High-frequency streaming data |

## Resilience

All Oura API requests are protected by a **standard resilience pipeline** powered by [Polly](https://github.com/App-vNext/Polly) via `Microsoft.Extensions.Http.Resilience`:

| Strategy | Configuration | Purpose |
|----------|---------------|---------|
| **Retry** | 3 attempts, exponential backoff + jitter | Handles 429 (rate limit), 500+, 408, and network errors |
| **Circuit breaker** | Opens at 10% failure rate, 5s break | Prevents hammering a failing API |
| **Attempt timeout** | 10s per request | Caps individual request duration |
| **Total timeout** | 30s including retries | Caps end-to-end request duration |

The resilience handler respects `Retry-After` headers from rate-limited responses. Auth errors (401) and permission errors (403) are handled separately by the client and are not retried.

## Technology Stack

- **.NET 10** / C# with nullable reference types
- **ModelContextProtocol** NuGet package (MCP C# SDK) — STDIO transport
- **System.Text.Json** for serialization
- **IHttpClientFactory** for HTTP client lifecycle management
- **IMemoryCache** for in-memory response caching with tiered TTLs
- **Microsoft.Extensions.Http.Resilience** (Polly) for retry, circuit breaker, and timeout policies

## License

MIT
