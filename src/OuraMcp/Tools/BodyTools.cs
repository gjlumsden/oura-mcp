using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class BodyTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves heart rate data from the Oura Ring.")]
    public async Task<string> GetHeartRate(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetHeartRateAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves heart rate variability data from the Oura Ring.")]
    public async Task<string> GetHeartRateVariability(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetHeartRateVariabilityAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves daily SpO2 blood oxygen data from the Oura Ring.")]
    public async Task<string> GetDailySpo2(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetDailySpo2Async(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves VO2 max estimates from the Oura Ring.")]
    public async Task<string> GetVo2Max(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetVo2MaxAsync(start, end);
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves cardiovascular age estimates from the Oura Ring.")]
    public async Task<string> GetCardiovascularAge(string? startDate = null, string? endDate = null)
    {
        var start = startDate is not null ? DateOnly.Parse(startDate) : (DateOnly?)null;
        var end = endDate is not null ? DateOnly.Parse(endDate) : (DateOnly?)null;
        var result = await client.GetCardiovascularAgeAsync(start, end);
        return JsonSerializer.Serialize(result);
    }
}
