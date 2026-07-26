using System.Collections.Concurrent;
using System.Text;
using BossHud.BossConfigs;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace BossHud.Bosses;

public sealed class BossManager(BossHudPlugin plugin) : IDisposable
{
    private readonly ConcurrentDictionary<IBoss, byte> _activeBosses = new();
    private readonly ConcurrentDictionary<CEntityInstance, byte> _bossEntities = new();
    private readonly HashSet<BossConfig> _bossesSeenThisRound = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private TaskCompletionSource? _waker;
    private bool _started;
    private bool _disposed;

    private IEnumerable<IBoss> ActiveBosses => _activeBosses.Keys;

    // TODO: Should a boss be triggerable again on the same round if multi-trigger is off?

    public string? BossMessage { get; private set; }

    public void Register(IBoss boss)
    {
        _activeBosses.TryAdd(boss, 0);
        _bossEntities.TryAdd(boss.MainEntity, 0);
        WakeIfNeeded();
    }

    public void Unregister(IBoss boss)
    {
        _bossEntities.TryRemove(boss.MainEntity, out _);
        _activeBosses.TryRemove(boss, out _);
    }

    /// <summary>
    /// Tries to mark a boss config as triggered for this round.
    /// </summary>
    /// <param name="config">The boss config</param>
    /// <returns>Whether this is the first time</returns>
    public bool TryMarkBossConfigAsTriggered(BossConfig config) => _bossesSeenThisRound.Add(config);

    public bool IsBossEntity(CEntityInstance instance) => _bossEntities.ContainsKey(instance);

    public void CleanupRound()
    {
        foreach (var activeBoss in ActiveBosses)
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
                if (_activeBosses.IsEmpty)
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
        if (!_activeBosses.IsEmpty) _waker?.TrySetResult();
    }

    private void UpdateMessage()
    {
        // TODO: Sort by config order?
        var activeBoss = ActiveBosses
            .FirstOrDefault(b => b.Shown); // TODO: Pick more intelligently or multi-show

        if (activeBoss is null)
        {
            BossMessage = null;
            return;
        }

        // TODO: cap at 100%
        var hpPercentage = activeBoss.CurrentHealth / (double)activeBoss.MaxHealth;
        var displaySegments = (int)Math.Round(hpPercentage * 20.0, MidpointRounding.AwayFromZero);
        var remaining = 20 - displaySegments;

        var builder = new StringBuilder();
        if (activeBoss.Config.Name is not null)
        {
            builder.Append($"<font>{activeBoss.Config.Name}</font><br>");
        }

        var color = Gradient.GetHpColor(hpPercentage);
        builder.Append(
            $"<font class=\"fontSize-l\" color=\"{color}\">{activeBoss.CurrentHealth}</font> <font>({hpPercentage * 100.0:N0}%)</font><br><font color=\"{color}\">");

        for (var i = 0; i < displaySegments; i++)
        {
            builder.Append('█');
        }

        builder.Append("</font><font color=\"gray\">");

        for (var i = 0; i < remaining; i++)
        {
            builder.Append('█');
        }

        builder.Append("</font>");

        BossMessage = builder.ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}