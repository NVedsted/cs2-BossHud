using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public class SegmentedCounterBossConfig : BaseCounterBossConfig
{
    public required EntityIdentifier IteratorIdentifier { get; init; }

    public int IteratorOffset { get; set; }

    public bool ReverseIterator { get; set; } = false;

    protected override OutputDefinitionWithDelay DefaultKillTrigger => new()
    {
        EntityIdentifier = IteratorIdentifier.AsTemplated(Templated),
        Output = ReverseIterator ? "OnHitMax" : "OnHitMin",
    };

    protected override IBoss CreateBoss(CreateBossContext context) =>
        new SegmentedCounterBoss<SegmentedCounterBossConfig>(
            context.WithConfig(this),
            context.GetEntity<CMathCounter>(IteratorIdentifier));
}