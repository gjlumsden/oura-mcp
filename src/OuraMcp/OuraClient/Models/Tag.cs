using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record Tag(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("day")] DateOnly? Day,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("timestamp")] DateTimeOffset? Timestamp,
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags
);
