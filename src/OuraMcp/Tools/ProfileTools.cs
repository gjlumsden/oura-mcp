using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OuraMcp.OuraClient;

namespace OuraMcp.Tools;

[McpServerToolType]
public class ProfileTools(IOuraApiClient client)
{
    [McpServerTool(Name = "get_personal_info"), Description("Retrieves personal info from the Oura Ring account.")]
    public async Task<string> GetPersonalInfo(CancellationToken cancellationToken = default)
    {
        var result = await client.GetPersonalInfoAsync(cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "get_ring_configuration"), Description("Retrieves ring configuration details from the Oura Ring.")]
    public async Task<string> GetRingConfiguration(CancellationToken cancellationToken = default)
    {
        var result = await client.GetRingConfigurationAsync(cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}
