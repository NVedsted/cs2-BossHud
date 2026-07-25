using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
#if HITMARKER
using HitMarkApi;
#endif

namespace BossHud.Bosses;

public abstract class Boss(BossConfig config, BossHudPlugin plugin) : IDisposable
{
    public BossConfig Config => config;

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    public bool Shown => true;

    public bool Disposed { get; private set; }

    protected DamageTable DamageTable { get; } = new();

    public abstract void Initialize();

    protected void OnHit(CCSPlayerController player)
    {
#if HITMARKER
        IHitMarkApi.Capability.Get()!.StartHitMark(player, false, null, null);
#endif
    }

    public virtual void Dispose()
    {
        plugin.BossManager.Unregister(this);
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    protected void Defeated()
    {
        plugin.Logger.LogInformation("Boss {BossName} has been defeated", Config.Name);
        // TODO: Enable once ready
        // DamageTable.Print(BossType);
        Dispose();
    }
}