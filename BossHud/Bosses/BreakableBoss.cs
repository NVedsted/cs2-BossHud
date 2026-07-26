using BossHud.BossConfigs;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class BreakableBoss(
    BreakableBossConfig config,
    BossHudPlugin plugin,
    CBreakable breakable)
    : Boss(config, plugin)
{
    public override BossType BossType => BossType.Hp;

    public override void Initialize()
    {
        ListenerScope.RegisterListener<Listeners.OnEntityTakeDamagePost>(OnEntityTakeDamagePost);
        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        ListenerScope.HookSingleEntityOutput(breakable, "OnBreak", OnBreak);
        ListenerScope.HookSingleEntityOutput(breakable, "OnHealthChanged", OnHealthChanged);
        Update();
    }

    private HookResult OnHealthChanged(
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

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (entity != breakable) return;

        Dispose();
    }

    private void OnEntityTakeDamagePost(CBaseEntity entity, CTakeDamageInfo info, CTakeDamageResult result)
    {
        if (entity != breakable) return;

        if (!info.Attacker.Value!.IsPlayerPawn()) return;

        PostDamage(info.Attacker.Value!.As<CCSPlayerPawn>().Controller.Value!.As<CCSPlayerController>(), info.Damage);
    }

    private HookResult OnBreak(
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

    private void Update()
    {
        CurrentHealth = breakable.Health;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    private void PostDamage(CCSPlayerController player, float damage)
    {
        DamageTable.AddDamage(player, damage);
        OnHit(player);
    }
}