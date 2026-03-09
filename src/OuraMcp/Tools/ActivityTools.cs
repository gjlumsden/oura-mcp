using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ActivityTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily activity scores and step counts from the Oura Ring.")]
    public async Task<string> GetDailyActivity(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailyActivityAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves workout data from the Oura Ring.")]
    public async Task<string> GetWorkouts(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetWorkoutsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves session data such as meditation and breathing exercises from the Oura Ring.")]
    public async Task<string> GetSessions(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetSessionsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
