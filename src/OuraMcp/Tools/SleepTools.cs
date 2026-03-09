using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class SleepTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_daily_sleep"), Description("Retrieves daily sleep scores and summaries from the Oura Ring.")]
    public async Task<string> GetDailySleep(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailySleepAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_sleep_periods"), Description("Retrieves detailed sleep period data from the Oura Ring.")]
    public async Task<string> GetSleepPeriods(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetSleepPeriodsAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_sleep_time"), Description("Retrieves recommended sleep time windows from the Oura Ring.")]
    public async Task<string> GetSleepTime(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetSleepTimeAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
