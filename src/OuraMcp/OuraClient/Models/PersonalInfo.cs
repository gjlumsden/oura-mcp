using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Oura API response for personal user information.</summary>
public record PersonalInfo
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("age")] public int? Age { get; init; }
    [JsonPropertyName("weight")] public double? Weight { get; init; }
    [JsonPropertyName("height")] public double? Height { get; init; }
    [JsonPropertyName("biological_sex")] public string? BiologicalSex { get; init; }
    [JsonPropertyName("email")] public string? Email { get; init; }
}
