using BossHud.BossConfigs;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SegmentedWithBackupCounterBoss(
    SegmentedWithBackupCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter,
    CMathCounter iterator,
    CMathCounter backup)
    : Boss(config, plugin)
{

    private void Update()
    {
        var currentSegmentHealth = (int)(counter.GetValue() - counter.Min);
        var segmentHealth = (int)backup.GetValue();
        var segmentsLeft = (int)(iterator.GetValue() - iterator.Min) - 1;

        CurrentHealth = segmentHealth * segmentsLeft + currentSegmentHealth;
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
        ListenerScope.HookSingleEntityOutput(counter, "OutValue", OnCounterOutValue);
        ListenerScope.HookSingleEntityOutput(iterator, "OutValue", OnOutValueUpdate);
        ListenerScope.HookSingleEntityOutput(backup, "OutValue", OnOutValueUpdate);
        ListenerScope.HookSingleEntityOutput(iterator, "OnHitMin", OnIteratorHitMin);
        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        Update();
    }

    private HookResult OnOutValueUpdate(
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
        if (entity != counter && entity != iterator) return;

        Dispose();
    }

    private HookResult OnIteratorHitMin(
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