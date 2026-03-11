using System.ComponentModel;
using ModelContextProtocol.Server;

namespace OuraMcp.Tools;

/// <summary>
/// MCP tools for diagnosing issues with the oura-mcp server.
/// </summary>
[McpServerToolType]
public class DiagnosticTools
{
    /// <summary>
    /// Reads and returns the contents of the error log file at <c>~/.oura-mcp/logs/error.log</c>.
    /// Returns the most recent entries, limited by the specified number of lines (default 100),
    /// so the AI assistant can diagnose failures.
    /// </summary>
    [McpServerTool(Name = "get_error_log"), Description(
        "Retrieves the oura-mcp error log. Use this to diagnose tool failures. " +
        "Returns the most recent error entries from ~/.oura-mcp/logs/error.log.")]
    public static string GetErrorLog(
        [Description("Maximum number of lines to return from the end of the log. Defaults to 100.")] int? tailLines = null)
        => ReadErrorLog(OuraMcpPaths.LogDirectory, tailLines);

    /// <summary>
    /// Reads error log entries from the specified log directory. Separated from the tool
    /// method to allow testing with a custom directory.
    /// </summary>
    internal static string ReadErrorLog(string logDirectory, int? tailLines = null)
    {
        if (!Directory.Exists(logDirectory))
        {
            return "No error log directory found. No errors have been recorded yet.";
        }

        // Find the most recent log file (Serilog rolling adds date suffixes)
        var logFiles = Directory.GetFiles(logDirectory, "error*.log")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        if (logFiles.Count == 0)
        {
            return "No error log files found. No errors have been recorded yet.";
        }

        var maxLines = Math.Clamp(tailLines ?? 100, 1, 1000);
        var lines = new List<string>();

        // Read from most recent file(s) until we have enough lines
        foreach (var file in logFiles)
        {
            try
            {
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                var fileLines = reader.ReadToEnd().Split('\n');
                lines.InsertRange(0, fileLines);

                if (lines.Count >= maxLines)
                {
                    break;
                }
            }
            catch (IOException)
            {
                // File may be locked by the logger — skip to the next one
            }
        }

        if (lines.All(string.IsNullOrWhiteSpace))
        {
            return "Error log is empty. No errors have been recorded yet.";
        }

        var result = lines
            .AsEnumerable()
            .Reverse()
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Take(maxLines)
            .Reverse();

        return string.Join('\n', result);
    }
}
