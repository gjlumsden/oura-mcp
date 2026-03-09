using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ProfileTools(IOuraApiClient client)
{
    [McpServerTool, Description("Retrieves personal info from the Oura Ring account.")]
    public async Task<string> GetPersonalInfo()
    {
        var result = await client.GetPersonalInfoAsync();
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool, Description("Retrieves ring configuration details from the Oura Ring.")]
    public async Task<string> GetRingConfiguration()
    {
        var result = await client.GetRingConfigurationAsync();
        return JsonSerializer.Serialize(result);
    }
}
