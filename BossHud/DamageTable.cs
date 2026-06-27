using BossHud.Bosses;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud;

public sealed class DamageTable
{
    private class Entry
    {
        public float Dmg { get; set; }

        public int Hits { get; set; }
    }

    // TODO: reset on disconnect
    private readonly Entry?[] _damageTable = new Entry?[64];

    public void AddDamage(CCSPlayerController player, float damage)
    {
        _damageTable[player.Slot] ??= new Entry();
        _damageTable[player.Slot]!.Dmg += damage;
        _damageTable[player.Slot]!.Hits++;
    }

    public void Print(BossType bossType)
    {
        foreach (var (dmg, slot) in _damageTable.Select((f, i) => (f, i)))
        {
            if (dmg is null) continue;

            var player = Utilities.GetPlayerFromSlot(slot);

            if (player is null) continue;

            switch (bossType)
            {
                case BossType.Hits:
                    Server.PrintToChatAll($"{player.PlayerName} - {dmg.Hits} hits");
                    break;
                case BossType.Hp:
                    Server.PrintToChatAll($"{player.PlayerName} - {dmg.Dmg} dmg ({dmg.Hits} hits)");
                    break;
                default:
                    throw new InvalidOperationException("Unknown boss type");
            }
        }
    }
}