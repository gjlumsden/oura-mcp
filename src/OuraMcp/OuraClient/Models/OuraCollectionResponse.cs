using System.Text.Json.Serialization;

namespace OuraMcp.OuraClient.Models;

public record OuraCollectionResponse<T>
{
    [JsonPropertyName("data")] public IReadOnlyList<T> Data { get; init; } = [];
    [JsonPropertyName("next_token")] public string? NextToken { get; init; }
}
