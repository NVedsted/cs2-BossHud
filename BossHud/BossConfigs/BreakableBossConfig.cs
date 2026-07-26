using BossHud.Bosses;

namespace BossHud.BossConfigs;

public sealed class BreakableBossConfig : BossConfig
{
    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OnHealthChanged"
    };

    protected override Boss CreateBoss(CreateBossContext context) => new BreakableBoss(context);
}