using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ActivityTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_daily_activity"), Description("Retrieves daily activity scores and step counts from the Oura Ring.")]
    public async Task<string> GetDailyActivity(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailyActivityAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_workouts"), Description("Retrieves workout data from the Oura Ring.")]
    public async Task<string> GetWorkouts(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetWorkoutsAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_sessions"), Description("Retrieves session data such as meditation and breathing exercises from the Oura Ring.")]
    public async Task<string> GetSessions(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetSessionsAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
