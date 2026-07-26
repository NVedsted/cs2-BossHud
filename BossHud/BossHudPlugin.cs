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

[MinimumApiVersion(370)]
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

    private BossConfig[] _activeBossConfigs = [];

    public BossManager BossManager { get; }

    private readonly CCSPlayerController?[] _activePlayers = new CCSPlayerController?[64];

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private ConfigState _mapConfigState = ConfigState.Valid;

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
        foreach (var bossConfig in _activeBossConfigs)
        {
            if (bossConfig.Trigger.EntityIdentifier.Matches(caller) && name == bossConfig.Trigger.Output)
            {
                bossConfig.SetupBoss(this, caller);
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
        switch (_mapConfigState)
        {
            case ConfigState.Unsupported:
                NotifyAdmins("The map configuration is not fully-supported; some bosses might not show as intended");
                break;
            case ConfigState.Broken:
                NotifyAdmins("The map configuration is broken; some bosses might not show as intended");
                break;
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
        _activeBossConfigs = [];
        _mapConfigState = ConfigState.Valid;

        var configName = Path.Combine(ConfigDirectory, $"maps/{mapName}.jsonc");

        if (File.Exists(configName))
        {
            Logger.LogInformation("Loading config for {MapName}", mapName);
            using var stream = File.OpenRead(configName);

            JsonEntry[] entries;
            try
            {
                var result = JsonSerializer.Deserialize<JsonEntry[]>(stream, JsonSerializerOptions);

                if (result is null)
                {
                    Logger.LogError("{ConfigName} is empty", configName);
                    _mapConfigState = ConfigState.Broken;
                    return;
                }

                entries = result;
            }
            catch (JsonException exception)
            {
                Logger.LogError(exception, "Failed to parse {ConfigName}", configName);
                return;
            }

            _activeBossConfigs = entries.Select(c =>
                {
                    try
                    {
                        return c.Parse();
                    }
                    catch (BossConfigException exception)
                    {
                        _mapConfigState = exception.ConfigState;
                        Logger.LogError(exception,
                            "A boss config for {MapName} could not be loaded (Boss: {BossName}, State: {State})",
                            mapName, exception.BossName, exception.ConfigState);
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