using System.Text.Json;
using BossHud.BossConfigs;
using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace BossHud;

[MinimumApiVersion(367)]
public class BossHudPlugin : BasePlugin, IPluginConfig<BossHudPluginConfig>
{
    public override string ModuleName => "BossHud";
    public override string ModuleVersion => "Nicklas Vedsted";

    private static readonly string ConfigDirectory =
        Path.Combine(Application.RootDirectory, "configs/plugins/BossHud");

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        TypeInfoResolver = BossHudJsonContext.Default,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    // TODO: commands for reloading and flushing active bosses?
    private BossConfig[] _activeBossConfigs = [];

    public BossManager BossManager { get; }

    private readonly CCSPlayerController?[] _activePlayers = new CCSPlayerController?[64];

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _invalidMapConfig;

    public BossHudPlugin()
    {
        BossManager = new(this);
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        HookEntityOutput("*", "*", OnEntityOutput);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(EventPlayerDisconnect);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnded);
        RegisterEventHandler<EventRoundStart>(EventRoundStart);

        if (hotReload)
        {
            OnMapStart(Server.MapName);

            foreach (var player in Utilities.GetPlayers())
            {
                _activePlayers[player.Slot] = player;
            }
        }
    }

    private HookResult OnEntityOutput(
        CEntityIOOutput output,
        string name,
        CEntityInstance activator,
        CEntityInstance caller,
        CVariant value,
        float delay)
    {
        // TODO: account for delay?

        var targetName = caller.Entity?.Name;

        if (targetName is null) return HookResult.Continue;

        foreach (var bossConfig in _activeBossConfigs)
        {
            if (targetName == bossConfig.Trigger.Entity && name == bossConfig.Trigger.Output)
            {
                bossConfig.SetupBoss(this);
            }
        }

        return HookResult.Continue;
    }

    private void CleanupRound()
    {
        BossManager.CleanupRound();
        CleanupRoundTimers();
    }

    private HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (_invalidMapConfig)
        {
            NotifyAdmins("The map configuration is not fully-supported; some bosses might not show as intended");
        }

        // Sometimes round end is not triggered properly.
        CleanupRound();

        return HookResult.Continue;
    }

    private HookResult EventRoundEnded(EventRoundEnd @event, GameEventInfo info)
    {
        CleanupRound();

        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        _cancellationTokenSource.Cancel();
    }

    private void OnMapStart(string mapName)
    {
        _invalidMapConfig = false;

        var configName = Path.Combine(ConfigDirectory, $"maps/{mapName}.jsonc");

        if (File.Exists(configName))
        {
            Logger.LogInformation("Loading config for {MapName}", mapName);
            using var stream = File.OpenRead(configName);
            var entries = JsonSerializer.Deserialize<Entry[]>(stream, JsonSerializerOptions);

            if (entries is null) throw new InvalidOperationException($"{configName} could not be parsed");

            _activeBossConfigs = entries.Select(c =>
                {
                    try
                    {
                        return c.Parse();
                    }
                    catch (EntryNotSupportedException exception)
                    {
                        _invalidMapConfig = true;
                        Logger.LogError(exception,
                            "A boss config for {MapName} uses unsupported configuration; ignoring", mapName);
                        return null;
                    }
                })
                .Where(c => c is not null)
                .Cast<BossConfig>()
                .ToArray();
        }
        else
        {
            Logger.LogInformation("Could not find config for {MapName} ({ConfigName})", mapName, configName);
        }
    }

    private static void NotifyAdmins(string message)
    {
        foreach (var admin in Utilities.GetPlayers().Where(p => AdminManager.PlayerHasPermissions(p, "@css/generic")))
        {
            admin.PrintToChat($"{{green}}[BossHud] {{default}}{message}".ReplaceColorTags());
        }
    }

    private HookResult EventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is { } player) _activePlayers[player.Slot] = null;

        return HookResult.Continue;
    }

    private HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is { } player) _activePlayers[player.Slot] = player;

        return HookResult.Continue;
    }

    private void OnTick()
    {
        if (BossManager.BossMessage is null) return;

        foreach (var player in _activePlayers)
        {
            player?.PrintToCenterHtml(BossManager.BossMessage);
        }
    }

    public void OnConfigParsed(BossHudPluginConfig config)
    {
        Config = config;
    }

    public required BossHudPluginConfig Config { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        CleanupRoundTimers();
        BossManager.Dispose();
    }

    private readonly LinkedList<Timer> _roundTimers = new();

    private void CleanupRoundTimers()
    {
        foreach (var timer in _roundTimers)
        {
            timer.Kill();
        }
        _roundTimers.Clear();
    }

    public void AddRoundTimer(float interval, Action callback)
    {
        _roundTimers.AddLast(new Timer(interval, callback, TimerFlags.STOP_ON_MAPCHANGE));
    }
}

public class BossHudPluginConfig : BasePluginConfig;