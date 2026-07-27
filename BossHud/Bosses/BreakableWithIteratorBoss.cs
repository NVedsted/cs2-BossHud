using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public class BreakableWithIteratorBoss(
    ICreateBossContextWithConfig<BreakableWithIteratorBossConfig> context,
    CMathCounter iterator)
    : BreakableBoss<BreakableWithIteratorBossConfig>(context)
{
    private int _segmentHealth;
    private int _segmentsLefts;

    public override BossType BossType => BossType.Hp;

    public override void Initialize()
    {
        ListenerScope.HookSingleEntityOutput(iterator, "OutValue", OnOutputUpdate);
        base.Initialize();
    }

    protected override bool IsDeletedEntityBreaking(CEntityInstance entity) =>
        base.IsDeletedEntityBreaking(entity) || entity == iterator;

    protected override void Update()
    {
        var currentSegmentHealth = GetCurrentHealth();
        var currentSegmentsLeft = GetCurrentSegmentsLeft();

        if (currentSegmentsLeft != _segmentsLefts)
        {
            _segmentsLefts = currentSegmentsLeft;
            _segmentHealth = currentSegmentHealth;
        }
        else if (currentSegmentHealth > _segmentHealth)
        {
            _segmentHealth = currentSegmentHealth;
        }

        CurrentHealth = _segmentHealth * _segmentsLefts + currentSegmentHealth;
        MaxHealth = Math.Max(CurrentHealth, MaxHealth);
    }

    private int GetCurrentSegmentsLeft() => (Config.ReverseIterator
        ? (int)(iterator.Max - iterator.GetValue()) - 1
        : (int)(iterator.GetValue() - iterator.Min) - 1) + Config.IteratorOffset;
}