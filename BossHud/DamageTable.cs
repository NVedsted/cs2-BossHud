using BossHud.Bosses;
using BossHud.Utility;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

namespace BossHud;

public sealed class DamageTable : IDisposable
{
    private readonly ListenerScope _listenerScope;

    private class EntryData
    {
        public required CCSPlayerController Player { get; init; }

        public int Dmg { get; set; }

        public int Hits { get; set; }
    }

    private readonly EntryData?[] _damageTable = new EntryData?[64];

    public DamageTable(BossHudPlugin plugin)
    {
        _listenerScope = new ListenerScope(plugin);
        _listenerScope.RegisterListener<Listeners.OnClientDisconnectPost>(OnClientDisconnectPost);
    }

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
            .Where(e => e.Player.IsValid && e.Dmg > 0)
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

    public void Dispose() => _listenerScope.Dispose();

    private void OnClientDisconnectPost(int slot) => _damageTable[slot] = null;
}