using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class SleepTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily sleep scores and summaries from the Oura Ring.")]
    public async Task<string> GetDailySleep(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailySleepAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves detailed sleep period data from the Oura Ring.")]
    public async Task<string> GetSleepPeriods(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetSleepPeriodsAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves recommended sleep time windows from the Oura Ring.")]
    public async Task<string> GetSleepTime(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetSleepTimeAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
