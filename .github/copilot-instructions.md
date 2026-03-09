# Copilot Instructions — Oura Ring MCP Server

## Project Overview

This is a **.NET MCP (Model Context Protocol) Server** that exposes Oura Ring health and wellness data as MCP tools. It uses the official **MCP C# SDK** (`ModelContextProtocol` NuGet package) and connects to the **Oura API v2** using **OAuth2** authentication. The server is designed to be consumed by MCP hosts such as GitHub Copilot, Claude, and other AI assistants.

## Rules

1. **Always consult documentation before writing or modifying code.** Use the **Web Search** and **Microsoft Learn** MCP tools to look up current API schemas, SDK patterns, and protocol details. The authoritative sources are:
   - Oura API v2 docs: https://cloud.ouraring.com/v2/docs
   - .NET MCP SDK: https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp
   - MCP Protocol spec: https://modelcontextprotocol.io/docs/getting-started/intro
2. Never guess at Oura API response shapes or MCP SDK method signatures — look them up first.
3. Never commit secrets, tokens, or credentials to source code.
4. **Keep the README up to date.** When adding features, changing configuration, or modifying the project structure, update `README.md` to reflect those changes.
5. **Follow Test-Driven Development (TDD).** Write failing tests first, then implement the minimum code to pass, then refactor. Every new feature or bug fix starts with a test.

## Authentication — OAuth2 (Required)

All Oura API access **must** use OAuth2 Authorization Code Grant. Personal access tokens are deprecated and must not be used.

- **Authorization URL:** `https://cloud.ouraring.com/oauth/authorize`
- **Token URL:** `https://api.ouraring.com/oauth/token`
- **Grant type:** `authorization_code`
- **Scopes (read-only, request all):** `email personal daily session heartrate tag workout ring_configuration`
- Tokens are passed as `Authorization: Bearer <token>` on every API request.
- Implement token refresh using the refresh token; handle 401 responses by triggering a refresh.

## Oura API v2 — Endpoints in Scope

All read endpoints under `https://api.ouraring.com/v2/usercollection/` are in scope:

| Category | Endpoint |
|---|---|
| Personal Info | `personal_info` |
| Ring Config | `ring_configuration` |
| Daily Sleep | `daily_sleep` |
| Daily Activity | `daily_activity` |
| Daily Readiness | `daily_readiness` |
| Daily Stress | `daily_stress` |
| Daily Resilience | `daily_resilience` |
| Sleep Periods | `sleep` |
| Sleep Time | `sleep_time` |
| Workouts | `workout` |
| Sessions | `session` |
| Heart Rate | `heartrate` |
| HRV | `heart_rate_variability` |
| SpO2 | `daily_spo2` |
| VO2 Max | `vo2_max` |
| Cardiovascular Age | `daily_cardiovascular_age` |
| Tags | `tag` |
| Enhanced Tags | `enhanced_tag` |
| Rest Mode | `rest_mode_period` |

Most collection endpoints accept `start_date`, `end_date`, and `next_token` query parameters. Paginate using `next_token` from the response until it is null.

## Technology Stack

- **.NET 10** / C# with nullable reference types enabled. Latest LTS version of .NET should be used.
- **ModelContextProtocol** NuGet package (MCP C# SDK) — use `--prerelease` if needed
- **Microsoft.Extensions.AI** for AI abstractions
- Expose each Oura data category as an MCP **Tool**
- Use `HttpClient` with typed response models for Oura API calls
- Use `IHttpClientFactory` for HttpClient lifecycle management
- Store OAuth credentials securely via user secrets or environment variables — never in source

## Conventions

- Follow standard .NET project layout (`src/`, solution file at root)
- Use `System.Text.Json` for serialization with source generators where appropriate
- Use C# record types for Oura API response models
- Async/await throughout; no sync-over-async
- XML doc comments on public APIs
