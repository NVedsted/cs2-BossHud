using System.Collections.Concurrent;

namespace BossHud.Bosses;

internal class BossManager
{
    private readonly ConcurrentDictionary<Boss, byte> _activeBosses = new();

    public IEnumerable<Boss> ActiveBosses => _activeBosses.Keys;

    public void Register(Boss boss)
    {
        _activeBosses.TryAdd(boss, 0);
    }

    public void Unregister(Boss boss)
    {
        _activeBosses.TryRemove(boss, out _);
    }
}