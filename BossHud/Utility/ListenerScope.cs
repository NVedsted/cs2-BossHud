using System.Reflection;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;

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
    }
}