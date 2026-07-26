using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public sealed class SegmentedCounterBoss(
    SegmentedCounterBossConfig config,
    BossHudPlugin plugin,
    CMathCounter counter,
    CMathCounter iterator)
    : BaseCounterBoss(config, plugin, counter)
{
    private int _segmentHealth;
    private int _segmentsLefts;

    protected override void Update()
    {
        var currentSegmentHealth = (int)(Counter.GetValue() - Counter.Min);
        var currentSegmentsLeft = (int)(iterator.GetValue() - iterator.Min) - 1;

        if (currentSegmentsLeft != _segmentsLefts)
        {
            _segmentsLefts = currentSegmentsLeft;
            _segmentHealth = currentSegmentHealth;
        }

        CurrentHealth = _segmentHealth * _segmentsLefts + currentSegmentHealth;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(iterator, "OutValue", OnOutputUpdate);
        ListenerScope.HookSingleEntityOutput(iterator, "OnHitMin", OnOutputDefeated);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return base.IsDeletedEntityBreaking(entity) || entity == iterator;
    }
}