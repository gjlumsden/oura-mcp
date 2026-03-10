using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class BodyTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_heart_rate"), Description("Retrieves heart rate data from the Oura Ring.")]
    public async Task<string> GetHeartRate(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetHeartRateAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_daily_spo2"), Description("Retrieves daily SpO2 blood oxygen data from the Oura Ring.")]
    public async Task<string> GetDailySpo2(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetDailySpo2Async(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_vo2_max"), Description("Retrieves VO2 max estimates from the Oura Ring.")]
    public async Task<string> GetVo2Max(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetVo2MaxAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_cardiovascular_age"), Description("Retrieves cardiovascular age estimates from the Oura Ring.")]
    public async Task<string> GetCardiovascularAge(
        [Description("Start date in yyyy-MM-dd format. Defaults to 7 days ago if not specified.")] string? startDate = null,
        [Description("End date in yyyy-MM-dd format. Defaults to today if not specified.")] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = DateHelper.ParseDate(startDate, nameof(startDate));
        var end = DateHelper.ParseDate(endDate, nameof(endDate));
        var result = await client.GetCardiovascularAgeAsync(start, end, ct: cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
