using BossHud.Bosses;

namespace BossHud.BossConfigs;

public sealed class SimpleCounterBossConfig : BaseCounterBossConfig
{
    protected override Boss CreateBoss(CreateBossContext context) => new SimpleCounterBoss(context);
}