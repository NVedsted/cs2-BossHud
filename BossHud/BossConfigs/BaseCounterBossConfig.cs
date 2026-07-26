namespace BossHud.BossConfigs;

public abstract class BaseCounterBossConfig : BossConfig
{
    protected sealed override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OutValue",
    };
}