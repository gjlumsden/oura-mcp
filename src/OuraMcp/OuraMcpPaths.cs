namespace OuraMcp;

/// <summary>
/// Shared paths for the oura-mcp data directory (<c>~/.oura-mcp</c>).
/// </summary>
internal static class OuraMcpPaths
{
    /// <summary>Root data directory: <c>~/.oura-mcp</c>.</summary>
    internal static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".oura-mcp");

    /// <summary>Error log directory: <c>~/.oura-mcp/logs</c>.</summary>
    internal static readonly string LogDirectory = Path.Combine(DataDirectory, "logs");

    /// <summary>Error log file path: <c>~/.oura-mcp/logs/error.log</c>.</summary>
    internal static readonly string ErrorLogPath = Path.Combine(LogDirectory, "error.log");
}
