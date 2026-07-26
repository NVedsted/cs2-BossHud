using BossHud.BossConfigs;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public abstract class BaseCounterBoss(
    BossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter) : Boss(config, plugin)
{
    public sealed override BossType BossType => BossType.Hits;

    protected CMathCounter Counter => counter;

    protected abstract void Update();

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(counter, "OutValue", OnCounterOutValue);
        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted); // TODO: Move to Boss
        Update();
    }

    private void PostDamage(CCSPlayerController player)
    {
        var oldHealth = CurrentHealth;
        Update();
        var damage = oldHealth - CurrentHealth;

        if (damage <= 0) return;

        // TODO: maybe move somewhere else
        DamageTable?.AddDamage(player, damage);
        OnHit(player);
    }

    protected HookResult OnOutputUpdate(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        Update();
        return HookResult.Continue;
    }

    protected virtual bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return entity == Counter;
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (!IsDeletedEntityBreaking(entity)) return;

        Dispose();
    }

    protected HookResult OnOutputDefeated(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        Server.NextFrame(Defeated);
        return HookResult.Continue;
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

        PostDamage(player);

        return HookResult.Continue;
    }
}