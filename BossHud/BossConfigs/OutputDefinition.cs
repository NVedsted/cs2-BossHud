using BossHud.EntityIdentifiers;

namespace BossHud.BossConfigs;

public record OutputDefinition
{
    public required EntityIdentifier EntityIdentifier { get; init; }

    public required string Output { get; init; }
}