using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class TagTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves tags from the Oura Ring.")]
    public async Task<string> GetTags(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetTagsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves enhanced tags from the Oura Ring.")]
    public async Task<string> GetEnhancedTags(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetEnhancedTagsAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
