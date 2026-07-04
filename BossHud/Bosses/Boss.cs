using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
#if HITMARKER
using HitMarkApi;
#endif

namespace BossHud.Bosses;

public abstract class Boss(BossHudPlugin plugin) : IDisposable
{
    public string? Name { get; init; }

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    public bool Active { get; set; }

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
        plugin.Logger.LogInformation("Boss {BossName} has been disposed", Name);
        plugin.UnregisterBoss(this);
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    protected void Defeated()
    {
        // TODO: Enable once ready
        // DamageTable.Print(BossType);
        Dispose();
    }
}