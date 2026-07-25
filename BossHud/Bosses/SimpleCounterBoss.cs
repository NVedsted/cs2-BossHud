using BossHud.BossConfigs;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SimpleCounterBoss(
    SimpleCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter)
    : Boss(config, plugin)
{
    private readonly BossHudPlugin _plugin = plugin;

    private void Update()
    {
        CurrentHealth = (int)counter.GetValue();
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    private void PostDamage(CCSPlayerController player)
    {
        var oldHealth = CurrentHealth;
        Update();
        var damage = oldHealth - CurrentHealth;

        if (damage <= 0) return;

        // TODO: maybe move somewhere else
        DamageTable.AddDamage(player, damage);
        OnHit(player);
    }

    public override BossType BossType => BossType.Hits;

    public override void Initialize()
    {
        _plugin.HookSingleEntityOutput(counter, "OutValue", OnCounterOutValue);
        _plugin.HookSingleEntityOutput(counter, "OnHitMin", OnCounterHitMin);
        _plugin.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        Update();
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (entity != counter) return;

        Dispose();
    }

    private HookResult OnCounterHitMin(
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

    public override void Dispose()
    {
        _plugin.UnhookSingleEntityOutput(counter, "OutValue", OnCounterOutValue);
        _plugin.UnhookSingleEntityOutput(counter, "OnHitMin", OnCounterHitMin);
        _plugin.RemoveListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        base.Dispose();
    }
}