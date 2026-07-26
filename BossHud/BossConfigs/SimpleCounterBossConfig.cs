using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SimpleCounterBossConfig : BossConfig
{
    public required EntityIdentifier CounterIdentifier { get; init; }

    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = CounterIdentifier,
        Output = "OutValue",
    };

    protected override Boss CreateBoss(BossHudPlugin plugin)
    {
        var counter = CounterIdentifier.Get()!.As<CMathCounter>(); // TODO: not found?

        return new SimpleCounterBoss(this, plugin, counter);
    }
}