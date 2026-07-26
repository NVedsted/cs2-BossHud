using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace BossHud.Utility;

public sealed class ListenerScope(BossHudPlugin plugin) : IDisposable
{
    private bool _disposed;

    private readonly LinkedList<(Type type, Delegate handler)> _registeredListeners = new();

    public void RegisterListener<T>(T handler) where T : Delegate
    {
        ThrowIfDisposed();
        _registeredListeners.AddLast((typeof(T), handler));
        plugin.RegisterListener(handler);
    }

    private readonly LinkedList<(
        CEntityInstance entityInstance,
        string outputName,
        EntityIO.EntityOutputHandler handler)> _singleEntityOutputHooks = new();

    public void HookSingleEntityOutput(
        CEntityInstance entityInstance,
        string outputName,
        EntityIO.EntityOutputHandler handler)
    {
        ThrowIfDisposed();
        _singleEntityOutputHooks.AddLast((entityInstance, outputName, handler));
        plugin.HookSingleEntityOutput(entityInstance, outputName, handler);
    }

    public void UnhookSingleEntityOutput(
        CEntityInstance entityInstance,
        string outputName,
        EntityIO.EntityOutputHandler handler)
    {
        ThrowIfDisposed();
        plugin.UnhookSingleEntityOutput(entityInstance, outputName, handler);
        _singleEntityOutputHooks.Remove((entityInstance, outputName, handler));
    }

    private readonly LinkedList<Timer> _timers = new();

    public void AddTimer(
        float interval,
        Action callback)
    {
        ThrowIfDisposed();
        _timers.AddLast(new Timer(interval, callback, TimerFlags.STOP_ON_MAPCHANGE));
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(ListenerScope));
    }

    private static readonly MethodInfo RemoveListenerMethod = typeof(BossHudPlugin).GetMethods()
        .Single(m => m is { Name: nameof(BossHudPlugin.RemoveListener), IsGenericMethodDefinition: true });

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        foreach (var listenerTypeGroup in _registeredListeners.GroupBy(l => l.type))
        {
            var removeMethod = RemoveListenerMethod.MakeGenericMethod(listenerTypeGroup.Key);
            foreach (var (_, handler) in listenerTypeGroup)
            {
                removeMethod.Invoke(plugin, [handler]);
            }
        }

        foreach (var (entityInstance, outputName, handler) in _singleEntityOutputHooks)
        {
            plugin.UnhookSingleEntityOutput(entityInstance, outputName, handler);
        }

        foreach (var timer in _timers)
        {
            timer.Kill();
        }
    }
}