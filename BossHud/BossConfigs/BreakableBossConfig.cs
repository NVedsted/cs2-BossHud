using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class BreakableBossConfig : BossConfig
{
    public required EntityIdentifier BreakableIdentifier { get; init; }

    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        EntityIdentifier = BreakableIdentifier,
        Output = "OnHealthChanged"
    };

    protected override Boss CreateBoss(BossHudPlugin plugin)
    {
        var breakable = BreakableIdentifier.Get()!.As<CBreakable>(); // TODO: not found?

        return new BreakableBoss(this, plugin, breakable);
    }
}