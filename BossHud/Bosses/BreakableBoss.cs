using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class BreakableBoss(ICreateBossContextWithConfig<BreakableBossConfig> context)
    : Boss<BreakableBossConfig>(context)
{
    private readonly CBreakable _breakable = context.MainEntity.As<CBreakable>();

    public override BossType BossType => BossType.Hp;

    public override void Initialize()
    {
        ListenerScope.RegisterListener<Listeners.OnEntityTakeDamagePost>(OnEntityTakeDamagePost);
        ListenerScope.HookSingleEntityOutput(_breakable, "OnHealthChanged", OnOutputUpdate);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity) => entity == _breakable;

    private void OnEntityTakeDamagePost(CBaseEntity entity, CTakeDamageInfo info, CTakeDamageResult result)
    {
        if (entity != _breakable) return;

        if (!info.Attacker.Value!.IsPlayerPawn()) return;

        var player = info.Attacker.Value!.As<CCSPlayerPawn>().Controller.Value!.As<CCSPlayerController>();
        DamageTable?.AddDamage(player, (int)info.Damage);
        OnHit(player);
    }

    protected override void Update()
    {
        CurrentHealth = _breakable.Health + Config.Offset;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }
}