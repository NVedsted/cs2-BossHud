using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedCounterBossConfig : BaseCounterBossConfig
{
    public required EntityIdentifier IteratorIdentifier { get; init; }

    protected override BaseCounterBoss CreateBoss(CreateBossContext context) => new SegmentedCounterBoss(
        context,
        context.GetEntity<CMathCounter>(IteratorIdentifier));
}