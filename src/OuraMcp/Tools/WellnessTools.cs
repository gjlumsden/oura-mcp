using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class WellnessTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily stress data from the Oura Ring.")]
    public async Task<string> GetDailyStress(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailyStressAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves daily resilience data from the Oura Ring.")]
    public async Task<string> GetDailyResilience(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailyResilienceAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves rest mode period data from the Oura Ring.")]
    public async Task<string> GetRestModePeriods(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetRestModePeriodsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
