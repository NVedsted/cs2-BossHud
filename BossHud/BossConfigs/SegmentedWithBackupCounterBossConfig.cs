using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedWithBackupCounterBossConfig : BossConfig
{
    public required string CounterName { get; init; }

    public required string IteratorName { get; init; }

    public required string BackupName { get; init; }

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

        var backup = Utilities.GetAllEntities()
            .First(e => e.Entity?.Name == BackupName)
            .As<CMathCounter>(); // TODO: not found?

        return new SegmentedWithBackupCounterBoss(this, plugin, counter, iterator, backup);
    }
}