using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ReadinessTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_daily_readiness"), Description("Retrieves daily readiness scores from the Oura Ring.")]
    public async Task<string> GetDailyReadiness(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailyReadinessAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
