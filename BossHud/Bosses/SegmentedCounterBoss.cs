using BossHud.BossConfigs;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SegmentedCounterBoss(
    SegmentedCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter,
    CMathCounter iterator)
    : Boss(config, plugin)
{
    private int _segmentHealth;
    private int _segmentsLefts;

    private void Update()
    {
        var currentSegmentHealth = (int)(counter.GetValue() - counter.Min);
        var currentSegmentsLeft = (int)(iterator.GetValue() - iterator.Min) - 1;

        if (currentSegmentsLeft != _segmentsLefts)
        {
            _segmentsLefts = currentSegmentsLeft;
            _segmentHealth = currentSegmentHealth;
        }

        CurrentHealth = _segmentHealth * _segmentsLefts + currentSegmentHealth;
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
        ListenerScope.HookSingleEntityOutput(iterator, "OutValue", OnIteratorOutValue);
        ListenerScope.HookSingleEntityOutput(iterator, "OnHitMin", OnIteratorHitMin);
        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        Update();
    }

    private HookResult OnIteratorOutValue(
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