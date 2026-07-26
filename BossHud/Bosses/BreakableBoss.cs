using BossHud.BossConfigs;
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
        ListenerScope.HookSingleEntityOutput(breakable, "OnBreak", OnOutputDefeated);
        ListenerScope.HookSingleEntityOutput(breakable, "OnHealthChanged", OnOutputUpdate);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return entity == breakable;
    }

    private void OnEntityTakeDamagePost(CBaseEntity entity, CTakeDamageInfo info, CTakeDamageResult result)
    {
        if (entity != breakable) return;

        if (!info.Attacker.Value!.IsPlayerPawn()) return;

        var player = info.Attacker.Value!.As<CCSPlayerPawn>().Controller.Value!.As<CCSPlayerController>();
        DamageTable?.AddDamage(player, (int)info.Damage);
        OnHit(player);
    }

    protected override void Update()
    {
        CurrentHealth = breakable.Health;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }
}