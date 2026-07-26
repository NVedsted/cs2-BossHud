namespace BossHud.BossConfigs;

// TODO: Merge simple and base?
public abstract class BaseCounterBossConfig : BossConfig
{
    protected sealed override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OutValue",
    };
}