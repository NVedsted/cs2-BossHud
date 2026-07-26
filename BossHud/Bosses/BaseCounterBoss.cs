using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public abstract class BaseCounterBoss(
    BossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter) : Boss(config, plugin)
{
    public sealed override BossType BossType => BossType.Hits;

    protected CMathCounter Counter => counter;

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(counter, "OutValue", OnCounterOutValue);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return entity == Counter;
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