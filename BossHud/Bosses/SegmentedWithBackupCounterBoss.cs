using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SegmentedWithBackupCounterBoss(
    ICreateBossContextWithConfig<SegmentedWithBackupCounterBossConfig> context,
    CMathCounter iterator,
    CMathCounter backup)
    : SegmentedCounterBoss<SegmentedWithBackupCounterBossConfig>(context, iterator)
{
    protected override void Update()
    {
        var currentSegmentHealth = (int)(Counter.GetValue() - Counter.Min);
        var segmentHealth = (int)backup.GetValue();
        var segmentsLeft = GetCurrentSegmentsLeft();

        CurrentHealth = segmentHealth * segmentsLeft + currentSegmentHealth;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(backup, "OutValue", OnOutputUpdate);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return base.IsDeletedEntityBreaking(entity) || entity == Iterator || entity == backup;
    }
}