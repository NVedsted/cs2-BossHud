namespace BossHud.BossConfigs;

// TODO: Merge simple and base?
public abstract class BaseCounterBossConfig : BossConfig
{
    public bool ReverseCounter { get; set; } = false;

    protected sealed override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OutValue",
    };
}