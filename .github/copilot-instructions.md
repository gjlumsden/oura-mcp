# Copilot Instructions — Oura Ring MCP Server

## Project Overview

.NET 10 MCP server exposing Oura Ring health data as MCP tools. Uses the **MCP C# SDK** (`ModelContextProtocol` NuGet) with **STDIO transport** and **CLI login (`az login` pattern)** to the Oura API v2. Users run `oura-mcp login` once to authenticate via browser (or `dotnet run --project src/OuraMcp -- login` from source); tokens are persisted to `~/.oura-mcp/tokens.json` and refreshed automatically.

## Architecture

- **Transport:** STDIO via `WithStdioServerTransport()`. Logging routes to stderr to avoid interfering with the transport.
- **Auth:** CLI login (`az login` pattern) — `oura-mcp login` (or `dotnet run --project src/OuraMcp -- login` from source) opens browser to Oura consent, exchanges the authorization code for tokens, and saves them to `~/.oura-mcp/tokens.json`. On subsequent runs the server loads tokens from disk and refreshes automatically when expired. `OURA_CLIENT_ID` / `OURA_CLIENT_SECRET` are read from env vars.
- **API calls:** All Oura v2 endpoints under `https://api.ouraring.com/v2/usercollection/` are in scope.

## Project Structure

```
src/OuraMcp/
  Program.cs              # Host setup, DI, MCP registration
  Auth/                   # OAuth options and token service
  OuraClient/             # IOuraApiClient + typed response models
    Models/               # C# record types for Oura API responses
  Tools/                  # MCP tool classes (one per domain)
tests/OuraMcp.Tests/      # Unit/integration tests
```

## Rules

1. **Consult docs before coding.** Authoritative sources:
   - Oura API v2: https://cloud.ouraring.com/v2/docs
   - .NET MCP SDK: https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp
   - MCP spec: https://modelcontextprotocol.io/docs/getting-started/intro
2. Never guess API shapes or SDK signatures — look them up.
3. Never commit secrets, tokens, or credentials.
4. Keep `README.md` up to date when changing features or structure.
5. **TDD:** Write a failing test first → implement minimum code to pass → refactor. Every feature or bug fix starts with a test.
6. **Branch and PR required.** `main` is protected — all changes require a feature branch and pull request. Never commit directly to `main`.

## DI / IoC Conventions

- **Interface-driven design.** All services have an interface (`IOuraApiClient`, `IOuraTokenService`).
- **`IHttpClientFactory`** with named clients (`"OuraApi"`, `"OuraAuth"`) — never `new HttpClient()`.
- **`IOptions<T>`** for configuration (`OuraOAuthOptions` bound from env vars).
- Tool classes use **primary constructor injection** (e.g., `SleepTools(IOuraApiClient client)`).
- Register MCP tools via `WithToolsFromAssembly()`.

## Authentication — CLI Login

- **Pattern:** `az login`-style — run `oura-mcp login` (or `dotnet run --project src/OuraMcp -- login` from source) to authorize via browser.
- **Callback:** Local `HttpListener` on `http://localhost:8742/callback/` receives the OAuth code.
- **Token storage:** `~/.oura-mcp/tokens.json` (owner-only permissions on Unix).
- **Refresh:** `OuraTokenService` checks expiry on each API call and refreshes automatically.
- **Scopes:** `email personal daily session heartrate tag workout spo2 ring_configuration`
- Tokens passed as `Authorization: Bearer <token>`.

## Distribution

- Published as a **NuGet .NET tool**: `dotnet tool install -g gjlumsden.OuraMcp` or `dnx -y gjlumsden.OuraMcp`.
- `PackageType` is `McpServer` for NuGet MCP discovery.
- Consumer-facing commands use the tool name (`oura-mcp login`, `oura-mcp`); dev commands use `dotnet run --project src/OuraMcp -- login` / `dotnet run --project src/OuraMcp`.
- README is consumer-first (NuGet install path); clone-and-build lives under the Development section.

## Tech Stack & Conventions

- **.NET 10** / C# with nullable reference types.
- **System.Text.Json** for serialization; source generators where appropriate.
- C# **record types** for API response models.
- Async/await throughout; no sync-over-async.
- XML doc comments on public APIs.
- Standard .NET layout: `src/`, `tests/`, solution file at root.
