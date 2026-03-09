using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class TagTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_tags"), Description("Retrieves tags from the Oura Ring.")]
    public async Task<string> GetTags(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetTagsAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_enhanced_tags"), Description("Retrieves enhanced tags from the Oura Ring.")]
    public async Task<string> GetEnhancedTags(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetEnhancedTagsAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
