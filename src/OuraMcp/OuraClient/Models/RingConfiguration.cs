using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record RingConfiguration
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("color")] public string? Color { get; init; }
    [JsonPropertyName("design")] public string? Design { get; init; }
    [JsonPropertyName("firmware_version")] public string? FirmwareVersion { get; init; }
    [JsonPropertyName("hardware_type")] public string? HardwareType { get; init; }
    [JsonPropertyName("set_up_at")] public DateTimeOffset? SetUpAt { get; init; }
    [JsonPropertyName("size")] public int? Size { get; init; }
}
