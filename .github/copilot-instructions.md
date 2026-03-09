# Copilot Instructions — Oura Ring MCP Server

## Project Overview

.NET 10 MCP server exposing Oura Ring health data as MCP tools. Uses the **MCP C# SDK** (`ModelContextProtocol` NuGet) with **HTTP transport + SSE** and **OAuth2 passthrough** to the Oura API v2. MCP hosts (GitHub Copilot, Claude, etc.) negotiate OAuth tokens with Oura directly; the server forwards the Bearer token on upstream API calls.

## Architecture

- **Transport:** HTTP + SSE via ASP.NET Core (`app.MapMcp("/mcp")`).
- **Auth:** OAuth2 passthrough — the MCP client handles the authorization code grant with Oura. The server reads `OURA_CLIENT_ID` / `OURA_CLIENT_SECRET` from env vars and never stores user tokens long-term.
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

## DI / IoC Conventions

- **Interface-driven design.** All services have an interface (`IOuraApiClient`, `IOuraTokenService`).
- **`IHttpClientFactory`** with named clients (`"OuraApi"`, `"OuraAuth"`) — never `new HttpClient()`.
- **`IOptions<T>`** for configuration (`OuraOAuthOptions` bound from env vars).
- Tool classes use **primary constructor injection** (e.g., `SleepTools(IOuraApiClient client)`).
- Register MCP tools via `WithToolsFromAssembly()`.

## Authentication — OAuth2

- **Authorization URL:** `https://cloud.ouraring.com/oauth/authorize`
- **Token URL:** `https://api.ouraring.com/oauth/token`
- **Grant type:** `authorization_code`
- **Scopes:** `email personal daily session heartrate tag workout spo2 ring_configuration`
- Tokens passed as `Authorization: Bearer <token>`. Handle 401 by triggering refresh.

## Tech Stack & Conventions

- **.NET 10** / C# with nullable reference types.
- **System.Text.Json** for serialization; source generators where appropriate.
- C# **record types** for API response models.
- Async/await throughout; no sync-over-async.
- XML doc comments on public APIs.
- Standard .NET layout: `src/`, `tests/`, solution file at root.
