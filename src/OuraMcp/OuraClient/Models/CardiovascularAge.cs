using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura daily cardiovascular age estimate derived from arterial health metrics.</summary>
public record CardiovascularAge
{
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("vascular_age")] public int? VascularAge { get; init; }
}
