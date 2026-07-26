using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SegmentedWithBackupCounterBoss(
    SegmentedWithBackupCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter,
    CMathCounter iterator,
    CMathCounter backup)
    : BaseCounterBoss(config, plugin, counter)
{
    protected override void Update()
    {
        var currentSegmentHealth = (int)(Counter.GetValue() - Counter.Min);
        var segmentHealth = (int)backup.GetValue();
        var segmentsLeft = (int)(iterator.GetValue() - iterator.Min) - 1;

        CurrentHealth = segmentHealth * segmentsLeft + currentSegmentHealth;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(iterator, "OutValue", OnOutputUpdate);
        ListenerScope.HookSingleEntityOutput(backup, "OutValue", OnOutputUpdate);
        ListenerScope.HookSingleEntityOutput(iterator, "OnHitMin", OnOutputDefeated);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return base.IsDeletedEntityBreaking(entity) || entity == iterator || entity == backup;
    }
}