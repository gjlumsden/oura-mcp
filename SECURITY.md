# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability, please report it responsibly:

1. **Do NOT open a public issue**
2. Email the maintainer or use GitHub's private vulnerability reporting feature
3. Include steps to reproduce the vulnerability
4. Allow reasonable time for a fix before public disclosure

## Supported Versions

| Version | Supported |
|---------|-----------|
| 0.x     | ✅ Current |

## Security Considerations

- OAuth tokens are encrypted at rest using .NET Data Protection (DPAPI on Windows, key-ring on Linux/macOS)
- Tokens are stored in `~/.oura-mcp/tokens.json` with restricted file permissions
- Client credentials (OURA_CLIENT_ID, OURA_CLIENT_SECRET) are passed via environment variables — never stored in source
- The server runs locally via STDIO transport — no network ports are opened during normal operation
