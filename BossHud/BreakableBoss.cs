using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud;

public enum BossType
{
    Hp,
    Hits,
}

public sealed class BreakableBoss(BossHudPlugin plugin, CBreakable breakable) : Boss(plugin)
{
    private readonly BossHudPlugin _plugin = plugin;
    public override BossType BossType => BossType.Hp;


    public override void Initialize()
    {
        _plugin.RegisterListener<Listeners.OnEntityTakeDamagePost>(OnEntityTakeDamagePost);
        _plugin.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        _plugin.HookSingleEntityOutput(breakable, "OnBreak", OnBreak);
        Update();
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
        base.Dispose();
    }

    private HookResult OnBreak(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        Server.NextFrame(Defeated);
        return HookResult.Continue;
    }

    private void Update()
    {
        CurrentHealth = breakable.Health;
        MaxHealth = breakable.MaxHealth;
    }

    private void PostDamage(CCSPlayerController player, float damage)
    {
        Active = true;
        Update();
        DamageTable.AddDamage(player, damage);
        OnHit(player);
    }
}