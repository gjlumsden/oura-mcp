using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record PersonalInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("age")] int? Age,
    [property: JsonPropertyName("weight")] double? Weight,
    [property: JsonPropertyName("height")] double? Height,
    [property: JsonPropertyName("biological_sex")] string? BiologicalSex,
    [property: JsonPropertyName("email")] string? Email
);
