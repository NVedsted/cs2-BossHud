using BossHud.BossConfigs;
using BossHud.Utility;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
#if HITMARKER
using CS2_HitMarkApi;
#endif

namespace BossHud.Bosses;

public abstract class Boss(CreateBossContext context) : IDisposable
{
    private bool _disposed;
    private readonly BossHudPlugin _plugin = context.Plugin;

    protected readonly ListenerScope ListenerScope = new(context.Plugin);

    public BossConfig Config => context.Config;

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    public bool Shown => true;

    public CEntityInstance MainEntity { get; } = context.MainEntity;

    protected DamageTable? DamageTable { get; } = context.Config.ShowBeaten ? new(context.Plugin) : null;

    public virtual void Initialize()
    {
        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        Update();
    }

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
        _plugin.BossManager.Unregister(this);
        GC.SuppressFinalize(this);
        _disposed = true;
        _plugin.Logger.LogInformation("Boss {BossName} was removed", Config.Name);
    }

    protected abstract bool IsDeletedEntityBreaking(CEntityInstance entity);

    protected HookResult OnOutputDefeated(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        Server.NextFrame(Defeated);
        return HookResult.Continue;
    }

    protected abstract void Update();

    protected HookResult OnOutputUpdate(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        Update();
        return HookResult.Continue;
    }

    private void Defeated()
    {
        _plugin.Logger.LogInformation("Boss {BossName} has been defeated", Config.Name);
        DamageTable?.Print(BossType, Config.Name);
        Dispose();
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (!IsDeletedEntityBreaking(entity)) return;

        Dispose();
    }
}