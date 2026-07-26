using BossHud.Bosses;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

namespace BossHud;

public sealed class DamageTable
{
    private class EntryData
    {
        public required CCSPlayerController Player { get; init; }

        public int Dmg { get; set; }

        public int Hits { get; set; }
    }

    // TODO: reset on disconnect
    private readonly EntryData?[] _damageTable = new EntryData?[64];

    public void AddDamage(CCSPlayerController player, int damage)
    {
        var entry = _damageTable[player.Slot] ??= new EntryData
        {
            Player = player,
        };
        entry.Dmg += damage;
        entry.Hits++;
    }

    public void Print(BossType bossType, string? bossName)
    {
        const int showTopCount = 5;

        var leaderboard = _damageTable.OfType<EntryData>()
            .Where(e => e.Player.IsValid)
            .OrderByDescending(entry => entry.Dmg)
            .Select((entry, index) => new
                { Rank = index + 1, entry.Player, entry.Player.PlayerName, entry.Dmg, entry.Hits })
            .ToArray();

        foreach (var row in TopDamageTable.BuildTopDamageTable(
                     bossType,
                     bossName,
                     leaderboard.Take(showTopCount).Select(e => (Name: e.PlayerName, e.Dmg, e.Hits))))
        {
            Server.PrintToChatAll(row.ReplaceColorTags());
        }

        foreach (var e in leaderboard.Skip(showTopCount))
        {
            e.Player.PrintToChat(TopDamageTable.BuildTopDamageRow(bossType, e.Rank, e.PlayerName, e.Dmg, e.Hits)
                .ReplaceColorTags());
        }
    }
}