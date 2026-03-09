using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class SleepTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily sleep scores and summaries from the Oura Ring.")]
    public async Task<string> GetDailySleep(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailySleepAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves detailed sleep period data from the Oura Ring.")]
    public async Task<string> GetSleepPeriods(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetSleepPeriodsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves recommended sleep time windows from the Oura Ring.")]
    public async Task<string> GetSleepTime(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetSleepTimeAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
