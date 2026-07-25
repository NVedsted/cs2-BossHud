using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SimpleCounterBossConfig : BossConfig
{
    public required string CounterName { get; init; }

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

        return new SimpleCounterBoss(this, plugin, counter);
    }
}