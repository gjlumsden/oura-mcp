using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class WellnessTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves daily stress data from the Oura Ring.")]
    public async Task<string> GetDailyStress(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailyStressAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves daily resilience data from the Oura Ring.")]
    public async Task<string> GetDailyResilience(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailyResilienceAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves rest mode period data from the Oura Ring.")]
    public async Task<string> GetRestModePeriods(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetRestModePeriodsAsync(start, end, cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
