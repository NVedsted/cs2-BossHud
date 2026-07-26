using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;

namespace BossHud.Bosses;

public sealed class SimpleCounterBoss(ICreateBossContextWithConfig<SimpleCounterBossConfig> context)
    : BaseCounterBoss<SimpleCounterBossConfig>(context)
{
    protected override void Update()
    {
        CurrentHealth = GetCounterValue();
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }
}