using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public abstract class BaseCounterBoss<TBossConfig>(ICreateBossContextWithConfig<TBossConfig> context)
    : Boss<TBossConfig>(context)
    where TBossConfig : BaseCounterBossConfig
{
    private readonly CMathCounter _counter = context.MainEntity.As<CMathCounter>();

    public sealed override BossType BossType => BossType.Hits;

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(_counter, "OutValue", OnCounterOutValue);
        base.Initialize();
    }

    protected int GetCounterValue() => Config.ReverseCounter
        ? (int)(_counter.Max - _counter.GetValue())
        : (int)(_counter.GetValue() - _counter.Min);

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return entity == _counter;
    }

    private HookResult OnCounterOutValue(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        if (activator.DesignerName != "player") return HookResult.Continue;

        var player = activator.As<CCSPlayerPawn>().Controller.Value!.As<CCSPlayerController>();

        var oldHealth = CurrentHealth;
        Update();
        var damage = oldHealth - CurrentHealth;

        if (damage <= 0) return HookResult.Continue;

        DamageTable?.AddDamage(player, damage);
        OnHit(player);

        return HookResult.Continue;
    }
}