using CounterStrikeSharp.API.Core;
using HitMarkApi;

namespace BossHud;

public abstract class Boss(BossHudPlugin plugin) : IDisposable
{
    public required string Name { get; init; }

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    public bool Active { get; set; }

    public bool Disposed { get; private set; }

    protected DamageTable DamageTable { get; } = new();

    public abstract void Initialize();

    protected void OnHit(CCSPlayerController player)
    {
        IHitMarkApi.Capability.Get()!.StartHitMark(player, false, null, null);
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    protected void Defeated()
    {
        DamageTable.Print(BossType);
        Dispose();
    }
}