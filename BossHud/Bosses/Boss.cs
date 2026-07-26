using BossHud.BossConfigs;
using BossHud.BossConfigs.Contexts;
using BossHud.Utility;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
#if HITMARKER
using CS2_HitMarkApi;
#endif

namespace BossHud.Bosses;

public abstract class Boss<TConfig>(ICreateBossContextWithConfig<TConfig> context) : IBoss
    where TConfig : BossConfig
{
    private bool _disposed;
    private readonly BossHudPlugin _plugin = context.Plugin;

    protected readonly ListenerScope ListenerScope = new(context.Plugin);

    BossConfig IBoss.Config => Config;

    protected TConfig Config => context.Config;

    public abstract BossType BossType { get; }

    public int CurrentHealth { get; protected set; }

    public int MaxHealth { get; protected set; }

    /// <summary>
    /// Timestamp of last time this boss was damaged.
    /// </summary>
    private long _lastDamaged;

    /// <summary>
    /// Whether this boss is currently timed out; cached to make timed-out bosses cheaper
    /// </summary>
    private bool _timedOut;

    // TODO: Pre-compute
    public bool Shown
    {
        get
        {
            if (Config.HitMarkerOnly || _timedOut) return false;

            // TODO: update max-health once shown?
            if (Config.MaxHp is not null && CurrentHealth > Config.MaxHp) return false;

            if (Config.Timeout is not null &&
                TimeProvider.System.GetElapsedTime(_lastDamaged).TotalSeconds > Config.Timeout.Value)
            {
                _timedOut = true;
                _plugin.Logger.LogInformation("Boss {BossName} has timed out", Config.Name);
                return false;
            }

            return true;
        }
    }

    public CEntityInstance MainEntity { get; } = context.MainEntity;

    protected DamageTable? DamageTable { get; } = context.Config.ShowBeaten ? new(context.Plugin) : null;

    public virtual void Initialize()
    {
        _lastDamaged = TimeProvider.System.GetTimestamp();

        // TODO: optimize?
        var killTriggerEntity = context.GetEntity<CEntityInstance>(Config.KillTrigger.EntityIdentifier);
        ListenerScope.HookSingleEntityOutput(killTriggerEntity, Config.KillTrigger.Output, OnKillTrigger);

        ListenerScope.RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        Update();
    }

    protected void OnHit(CCSPlayerController player)
    {
        _lastDamaged = TimeProvider.System.GetTimestamp();

        if (_timedOut)
        {
            _timedOut = false;
            _plugin.Logger.LogInformation("Boss {BossName} is no longer timed out", Config.Name);
        }
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

    private HookResult OnKillTrigger(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        if (Config.KillTrigger.Delay is null)
        {
            Server.NextFrame(Defeated);
        }
        else
        {
            ListenerScope.AddTimer(Config.KillTrigger.Delay.Value, Defeated);
        }

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