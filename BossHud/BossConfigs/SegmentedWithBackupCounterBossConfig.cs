using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public sealed class SegmentedWithBackupCounterBossConfig : BaseCounterBossConfig
{
    public required EntityIdentifier IteratorIdentifier { get; init; }

    public required EntityIdentifier BackupIdentifier { get; init; }

    protected override Boss CreateBoss(CreateBossContext context) => new SegmentedWithBackupCounterBoss(
        context,
        context.GetEntity<CMathCounter>(IteratorIdentifier),
        context.GetEntity<CMathCounter>(BackupIdentifier));
}