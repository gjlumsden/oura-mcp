using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record CardiovascularAge(
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("vascular_age")] int? VascularAge
);
