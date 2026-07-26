using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;

namespace BossHud.BossConfigs;

public sealed class SimpleCounterBossConfig : BaseCounterBossConfig
{
    protected override OutputDefinitionWithDelay DefaultKillTrigger => new()
    {
        EntityIdentifier = MainIdentifier.AsTemplated(Templated),
        Output = ReverseCounter ? "OnHitMax" : "OnHitMin",
    };

    protected override IBoss CreateBoss(CreateBossContext context) => new SimpleCounterBoss(context.WithConfig(this));
}