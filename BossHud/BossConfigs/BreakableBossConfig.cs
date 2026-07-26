using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;

namespace BossHud.BossConfigs;

public sealed class BreakableBossConfig : BossConfig
{
    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OnHealthChanged"
    };

    protected override OutputDefinitionWithDelay DefaultKillTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = "OnBreak"
    };

    protected override IBoss CreateBoss(CreateBossContext context) => new BreakableBoss(context.WithConfig(this));
}