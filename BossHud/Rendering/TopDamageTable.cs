using System.ComponentModel;
using BossHud.Bosses;

namespace BossHud.Rendering;

public static class TopDamageTable
{
    private static readonly string[] TopColors =
    [
        "{gold}", "{grey}", "{yellow}",
    ];

    public static IEnumerable<string> BuildTopDamageTable(
        BossType type,
        string? bossName,
        IEnumerable<(string Name, int Damage, int Hits)> entries)
    {
        yield return $"{{yellow}}TOP BOSS DAMAGE{(bossName is null ? "" : $" - {bossName}")}";

        foreach (var (rank, name, damage, hits) in entries.Select((entry, index) =>
                     (index + 1, entry.Name, entry.Damage, entry.Hits)))
        {
            yield return BuildTopDamageRow(type, rank, name, damage, hits);
        }
    }

    public static string BuildTopDamageRow(BossType type, int rank, string name, int damage, int hits)
    {
        if (rank < 1)
            throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank must be greater than or equal to 1");

        var color = rank <= 3 ? TopColors[rank - 1] : "{blue}";
        var unit = (type, damage) switch
        {
            (BossType.Hp, _) => "DMG",
            (BossType.Hits, not 1) => "HITS",
            (BossType.Hits, 1) => "HIT",
            (_, _) => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(BossType)),
        };
        var hitStats = type == BossType.Hp ? $" {{olive}}({hits} HIT{(hits == 1 ? "" : "S")})" : "";

        return $"{color}{rank}. {name} {{default}}- {{red}}{damage} {unit}{hitStats}";
    }
}