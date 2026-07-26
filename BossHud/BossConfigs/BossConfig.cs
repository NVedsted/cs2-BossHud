using BossHud.Bosses;
using BossHud.Model;
using Microsoft.Extensions.Logging;

namespace BossHud.BossConfigs;

public abstract class BossConfig
{
    public string? Name { get; set; }

    public OutputDefinitionWithDelay Trigger
    {
        get => field ??= DefaultTrigger;
        set;
    }

    protected abstract OutputDefinitionWithDelay DefaultTrigger { get; }

    public bool ShowBeaten { get; set; } = true;

    public void SetupBoss(BossHudPlugin plugin)
    {
        // Mark immediately as seen to not spam timers.
        if (!plugin.BossManager.BossesSeenThisRound.Add(this)) return;

        if (Trigger.Delay.HasValue)
        {
            plugin.AddRoundTimer(Trigger.Delay.Value, () => SetupBossInternal(plugin));
        }
        else
        {
            SetupBossInternal(plugin);
        }
    }

    private void SetupBossInternal(BossHudPlugin plugin)
    {
        plugin.Logger.LogInformation("Setting up boss {BossName}", Name ?? "<no-name>");
        var boss = CreateBoss(plugin);

        boss.Initialize();
        plugin.BossManager.Register(boss);
    }

    protected abstract Boss CreateBoss(BossHudPlugin plugin);
}