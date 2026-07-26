using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public class SegmentedCounterBossConfig : BaseCounterBossConfig
{
    public required EntityIdentifier IteratorIdentifier { get; init; }

    public bool ReverseIterator { get; set; } = false;

    protected override IBoss CreateBoss(CreateBossContext context) =>
        new SegmentedCounterBoss<SegmentedCounterBossConfig>(
            context.WithConfig(this),
            context.GetEntity<CMathCounter>(IteratorIdentifier));
}