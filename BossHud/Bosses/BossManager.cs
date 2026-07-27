using System.Collections.Immutable;
using BossHud.BossConfigs;
using BossHud.Rendering;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace BossHud.Bosses;

public sealed class BossManager(BossHudPlugin plugin) : IDisposable
{
    /// <summary>
    /// Read from another thread so much support thread-safe reading
    /// </summary>
    private ImmutableSortedSet<IBoss> _activeBosses = ImmutableSortedSet.Create<IBoss>(BossOrderComparer.Instance);

    private readonly HashSet<CEntityInstance> _bossEntities = new();
    private readonly HashSet<BossConfig> _bossesSeenThisRound = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private TaskCompletionSource? _waker;
    private bool _started;
    private bool _disposed;

    // TODO: Should a boss be triggerable again on the same round if multi-trigger is off?

    public string? BossMessage { get; private set; }

    public void Register(IBoss boss)
    {
        _activeBosses = _activeBosses.Add(boss);
        _bossEntities.Add(boss.MainEntity);
        WakeIfNeeded();
    }

    public void Unregister(IBoss boss)
    {
        _bossEntities.Remove(boss.MainEntity);
        _activeBosses = _activeBosses.Remove(boss);
    }

    /// <summary>
    /// Tries to mark a boss config as triggered for this round.
    /// </summary>
    /// <param name="config">The boss config</param>
    /// <returns>Whether this is the first time</returns>
    public bool TryMarkBossConfigAsTriggered(BossConfig config) => _bossesSeenThisRound.Add(config);

    public bool IsBossEntity(CEntityInstance instance) => _bossEntities.Contains(instance);

    public void CleanupRound()
    {
        foreach (var activeBoss in _activeBosses)
        {
            activeBoss.Dispose();
        }

        _bossesSeenThisRound.Clear();
        _bossEntities.Clear();
    }

    private void EnsureMessageUpdaterStarted()
    {
        if (_started) return;

        if (Interlocked.CompareExchange(ref _started, true, false)) return;

        var token = _cancellationTokenSource.Token;
        // Runs in worker thread
        Task.Run(async () =>
        {
            plugin.Logger.LogInformation("Starting background worker");
            while (!token.IsCancellationRequested)
            {
                if (_activeBosses.Count == 0)
                {
                    BossMessage = null;
                    _waker = new TaskCompletionSource();
                    WakeIfNeeded(); // In case a boss was registered after beginning to pause
                    plugin.Logger.LogInformation("Background worker has paused");
                    await _waker.Task;
                    plugin.Logger.LogInformation("Background worker has unpaused");
                }

                UpdateMessage();
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
            }

            plugin.Logger.LogInformation("Background worker has stopped");
        }, token);
    }

    private void WakeIfNeeded()
    {
        EnsureMessageUpdaterStarted();
        if (_activeBosses.Count != 0) _waker?.TrySetResult();
    }

    private void UpdateMessage()
    {
        BossMessage = Hud.BuildHud(_activeBosses
            .Where(b => b.Shown)
            .Select(b => new Hud.BossData(
                b.Config.Name,
                b.CurrentHealth,
                b.MaxHealth,
                b.Config.MinorHud,
                b.BossType)));
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}