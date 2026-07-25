using System.Text.Json.Serialization;

namespace BossHud.Model;

public record OutputDefinitionWithDelay : OutputDefinition
{
    [JsonPropertyName("delay")]
    public float? Delay { get; init; }
}
