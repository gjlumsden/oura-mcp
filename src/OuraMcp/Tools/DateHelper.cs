using ModelContextProtocol;

namespace OuraMcp.Tools;

/// <summary>
/// Shared date-parsing utilities for MCP tool methods.
/// </summary>
internal static class DateHelper
{
    /// <summary>
    /// Parses a date string in yyyy-MM-dd format, returning <c>null</c> when <paramref name="date"/> is <c>null</c>.
    /// Throws <see cref="McpException"/> with a user-friendly message when the format is invalid.
    /// </summary>
    internal static DateOnly? ParseDate(string? date, string paramName)
    {
        if (date is null)
        {
            return null;
        }

        if (!DateOnly.TryParse(date, out var result))
        {
            throw new McpException(
                $"Invalid date format for {paramName}: '{date}'. Expected yyyy-MM-dd.");
        }

        return result;
    }
}
