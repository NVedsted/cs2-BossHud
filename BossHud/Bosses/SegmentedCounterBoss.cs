using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public class SegmentedCounterBoss<TConfig>(
    ICreateBossContextWithConfig<TConfig> context,
    CMathCounter iterator)
    : BaseCounterBoss<TConfig>(context) where TConfig : SegmentedCounterBossConfig
{
    private int _segmentHealth;
    private int _segmentsLefts;

    protected readonly CMathCounter Iterator = iterator;

    protected int GetCurrentSegmentsLeft() => Config.ReverseIterator
        ? (int)(Iterator.GetValue() - Iterator.Min) - 1
        : (int)(Iterator.Max - Iterator.GetValue()) - 1;

    protected override void Update()
    {
        var currentSegmentHealth = (int)(Counter.GetValue() - Counter.Min);
        var currentSegmentsLeft = GetCurrentSegmentsLeft();

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
        ListenerScope.HookSingleEntityOutput(Iterator, "OutValue", OnOutputUpdate);
        ListenerScope.HookSingleEntityOutput(Iterator, "OnHitMin", OnOutputDefeated);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity)
    {
        return base.IsDeletedEntityBreaking(entity) || entity == Iterator;
    }
}