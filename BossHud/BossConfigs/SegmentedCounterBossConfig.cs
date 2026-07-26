using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedCounterBossConfig : BossConfig
{
    public required EntityIdentifier CounterIdentifier { get; init; }

    public required EntityIdentifier IteratorIdentifier { get; init; }

    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = CounterIdentifier,
        Output = "OutValue",
    };

    protected override Boss CreateBoss(BossHudPlugin plugin)
    {
        var counter = CounterIdentifier.Get()!.As<CMathCounter>(); // TODO: not found?
        var iterator = IteratorIdentifier.Get()!.As<CMathCounter>(); // TODO: not found?

        return new SegmentedCounterBoss(this, plugin, counter, iterator);
    }
}