using System.Text.Json.Serialization;

namespace BossHud.Model;

public record OutputDefinitionWithDelay : OutputDefinition
{
    /// <summary>
    /// Delay to wait after output
    /// </summary>
    [JsonPropertyName("delay")]
    public float? Delay { get; init; }
}
