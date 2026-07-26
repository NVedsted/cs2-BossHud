using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;

namespace BossHud.Bosses;

public interface IBoss : IDisposable
{
    BossConfig Config { get; }

    int CurrentHealth { get; }

    int MaxHealth { get; }

    bool Shown { get; }

    CEntityInstance MainEntity { get; }

    void Initialize();
}