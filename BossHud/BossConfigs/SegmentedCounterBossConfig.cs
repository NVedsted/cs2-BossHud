using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedCounterBossConfig : BossConfig
{
    public required string CounterName { get; init; }

    public required string IteratorName { get; init; }

    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        Entity = CounterName,
        Output = "OutValue",
    };

    protected override Boss CreateBoss(BossHudPlugin plugin)
    {
        var counter = Utilities.GetAllEntities()
            .First(e => e.Entity?.Name == CounterName)
            .As<CMathCounter>(); // TODO: not found?

        var iterator = Utilities.GetAllEntities()
            .First(e => e.Entity?.Name == IteratorName)
            .As<CMathCounter>(); // TODO: not found?

        return new SegmentedCounterBoss(this, plugin, counter, iterator);
    }
}