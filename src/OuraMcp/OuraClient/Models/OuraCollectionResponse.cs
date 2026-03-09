using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record OuraCollectionResponse<T>(
    [property: JsonPropertyName("data")] IReadOnlyList<T> Data,
    [property: JsonPropertyName("next_token")] string? NextToken
);
