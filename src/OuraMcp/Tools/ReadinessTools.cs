using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ReadinessTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily readiness scores from the Oura Ring.")]
    public async Task<string> GetDailyReadiness(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailyReadinessAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
