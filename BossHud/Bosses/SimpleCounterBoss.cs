using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SimpleCounterBoss(
    SimpleCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter)
    : BaseCounterBoss(config, plugin, counter)
{
    protected override void Update()
    {
        CurrentHealth = (int)(Counter.GetValue() - Counter.Min);
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(Counter, "OnHitMin", OnOutputDefeated);
        base.Initialize();
    }
}