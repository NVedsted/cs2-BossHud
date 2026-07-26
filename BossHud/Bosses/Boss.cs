using BossHud.BossConfigs;
using BossHud.Utility;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
#if HITMARKER
using CS2_HitMarkApi;
#endif

namespace BossHud.Bosses;

public abstract class Boss(BossConfig config, BossHudPlugin plugin) : IDisposable
{
    private bool _disposed;

    protected readonly ListenerScope ListenerScope = new(plugin);

    public BossConfig Config => config;

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    public bool Shown => true;

    protected DamageTable? DamageTable { get; } = config.ShowBeaten ? new(plugin) : null;

    public abstract void Initialize();

    protected void OnHit(CCSPlayerController player)
    {
#if HITMARKER
        IHitMarkApi.Capability.Get()!.StartHitMark(player, false, null, null);
#endif
    }

    public void Dispose()
    {
        if (_disposed) return;

        DamageTable?.Dispose();
        ListenerScope.Dispose();
        plugin.BossManager.Unregister(this);
        GC.SuppressFinalize(this);
        _disposed = true;
        plugin.Logger.LogInformation("Boss {BossName} was removed", Config.Name);
    }

    protected void Defeated()
    {
        plugin.Logger.LogInformation("Boss {BossName} has been defeated", Config.Name);
        DamageTable?.Print(BossType, Config.Name);
        Dispose();
    }
}