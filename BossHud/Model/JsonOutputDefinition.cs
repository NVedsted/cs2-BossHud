using System.Text.Json.Serialization;
using BossHud.BossConfigs;
using BossHud.EntityIdentifiers;

namespace BossHud.Model;

public record JsonOutputDefinition
{
    /// <summary>
    /// Targetname/Hammerid of entity
    /// </summary>
    [JsonPropertyName("ent")]
    public required string Entity { get; init; }

    /// <summary>
    /// Output of entity
    /// </summary>
    [JsonPropertyName("output")]
    public required string Output { get; init; }

    public OutputDefinition Parse(bool templated) => new()
        { EntityIdentifier = EntityIdentifier.Parse(Entity, templated), Output = Output };
}