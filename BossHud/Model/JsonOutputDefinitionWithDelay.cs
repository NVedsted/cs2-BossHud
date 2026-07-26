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

    public new OutputDefinitionWithDelay Parse(bool templated) => new()
        { EntityIdentifier = EntityIdentifier.Parse(Entity).AsTemplated(templated), Output = Output, Delay = Delay };
}