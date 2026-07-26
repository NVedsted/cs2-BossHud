namespace BossHud.BossConfigs;

public record OutputDefinitionWithDelay : OutputDefinition
{
    public float? Delay { get; init; }
}
