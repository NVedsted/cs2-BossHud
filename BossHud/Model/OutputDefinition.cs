using System.Text.Json.Serialization;

namespace BossHud.Model;

public record OutputDefinition
{
    [JsonPropertyName("ent")]
    public required string Entity { get; init; }

    [JsonPropertyName("output")]
    public required string Output { get; init; }
}