using BossHud.BossConfigs;

namespace BossHud.Bosses;

public sealed class SimpleCounterBoss(CreateBossContext context) : BaseCounterBoss(context)
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