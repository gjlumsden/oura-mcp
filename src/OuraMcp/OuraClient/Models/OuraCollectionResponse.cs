using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

/// <summary>Paginated collection response wrapper returned by Oura API v2 list endpoints.</summary>
public record OuraCollectionResponse<T>
{
    [JsonPropertyName("data")] public IReadOnlyList<T> Data { get; init; } = [];
    [JsonPropertyName("next_token")] public string? NextToken { get; init; }
}
