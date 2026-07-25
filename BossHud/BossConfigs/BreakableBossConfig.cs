using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class BreakableBossConfig : BossConfig
{
    public required string BreakableName { get; init; }

    protected override OutputDefinitionWithDelay DefaultTrigger => new()
    {
        Entity = BreakableName,
        Output = "OnHealthChanged"
    };

    protected override Boss CreateBoss(BossHudPlugin plugin)
    {
        var breakable = Utilities.GetAllEntities()
            .First(e => e.Entity?.Name == BreakableName)
            .As<CBreakable>(); // TODO: not found?

        return new BreakableBoss(this, plugin, breakable);
    }
}