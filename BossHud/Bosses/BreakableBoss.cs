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
    private readonly BossHudPlugin _plugin = plugin;
    public override BossType BossType => BossType.Hp;


    public override void Initialize()
    {
        _plugin.RegisterListener<Listeners.OnEntityTakeDamagePost>(OnEntityTakeDamagePost);
        _plugin.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        _plugin.HookSingleEntityOutput(breakable, "OnBreak", OnBreak);
        _plugin.HookSingleEntityOutput(breakable, "OnHealthChanged", OnHealthChanged);
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

    public override void Dispose()
    {
        _plugin.RemoveListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        _plugin.RemoveListener<Listeners.OnEntityTakeDamagePost>(OnEntityTakeDamagePost);
        _plugin.UnhookSingleEntityOutput(breakable, "OnBreak", OnBreak);
        _plugin.UnhookSingleEntityOutput(breakable, "OnHealthChanged", OnHealthChanged);
        base.Dispose();
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