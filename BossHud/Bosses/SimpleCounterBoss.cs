using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;

namespace BossHud.Bosses;

public sealed class SimpleCounterBoss(ICreateBossContextWithConfig<SimpleCounterBossConfig> context)
    : BaseCounterBoss<SimpleCounterBossConfig>(context)
{
    protected override void Update()
    {
        CurrentHealth = (int)(Counter.GetValue() - Counter.Min);
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }
}