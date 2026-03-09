using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record CardiovascularAge
{
    [JsonPropertyName("day")] public DateOnly? Day { get; init; }
    [JsonPropertyName("vascular_age")] public int? VascularAge { get; init; }
}
