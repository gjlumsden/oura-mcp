using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record RingConfiguration(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("color")] string? Color,
    [property: JsonPropertyName("design")] string? Design,
    [property: JsonPropertyName("firmware_version")] string? FirmwareVersion,
    [property: JsonPropertyName("hardware_type")] string? HardwareType,
    [property: JsonPropertyName("set_up_at")] DateTimeOffset? SetUpAt,
    [property: JsonPropertyName("size")] int? Size
);
