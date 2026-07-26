using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;

namespace BossHud.BossConfigs;

public sealed class SimpleCounterBossConfig : BaseCounterBossConfig
{
    protected override IBoss CreateBoss(CreateBossContext context) => new SimpleCounterBoss(context.WithConfig(this));
}