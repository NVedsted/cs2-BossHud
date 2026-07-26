using BossHud.BossConfigs.Contexts;
using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedWithBackupCounterBossConfig : SegmentedCounterBossConfig
{
    public required EntityIdentifier BackupIdentifier { get; init; }

    protected override IBoss CreateBoss(CreateBossContext context) => new SegmentedWithBackupCounterBoss(
        context.WithConfig(this),
        context.GetEntity<CMathCounter>(IteratorIdentifier),
        context.GetEntity<CMathCounter>(BackupIdentifier));
}