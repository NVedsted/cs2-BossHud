using System.Text.Json.Serialization;
using BossHud.BossConfigs;
using BossHud.EntityIdentifiers;

namespace BossHud.Model;

public record JsonOutputDefinitionWithDelay : JsonOutputDefinition
{
    /// <summary>
    /// Delay to wait after output
    /// </summary>
    [JsonPropertyName("delay")]
    public float? Delay { get; init; }

    public new OutputDefinitionWithDelay Parse() => new()
        { EntityIdentifier = Entity.AsTargetNameEntityIdentifier(), Output = Output, Delay = Delay };
}