using System.Text.Json;
using BossHud.BossConfigs;
using BossHud.Bosses;
using BossHud.Model;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;

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

    private List<BossConfig> _activeBossConfigs = [];

    private readonly BossManager _bossManager;

    private readonly CCSPlayerController?[] _activePlayers = new CCSPlayerController?[64];

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public BossHudPlugin()
    {
        _bossManager = new(this);
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(EventPlayerDisconnect);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnded);

        if (hotReload)
        {
            OnMapStart(Server.MapName);

            foreach (var player in Utilities.GetPlayers())
            {
                _activePlayers[player.Slot] = player;
            }

            foreach (var entity in Utilities.GetAllEntities())
            {
                SetupBoss(entity);
            }
        }
    }

    private HookResult EventRoundEnded(EventRoundEnd @event, GameEventInfo info)
    {
        // TODO: needed as entity-removed event disposes? Perhaps good in any case.
        foreach (var activeBoss in _bossManager.ActiveBosses)
        {
            activeBoss.Dispose();
        }

        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        _cancellationTokenSource.Cancel();
    }

    private void OnMapStart(string mapName)
    {
        Logger.LogInformation("Loading configs for  {MapName}", mapName);

        var configName = Path.Combine(ConfigDirectory, $"maps/{mapName}.jsonc");

        if (File.Exists(configName))
        {
            var entries = JsonSerializer.Deserialize<Entry[]>(File.OpenRead(configName), JsonSerializerOptions);

            if (entries is null) throw new InvalidOperationException($"{configName} could not be parsed");

            _activeBossConfigs = entries.Select(c =>
                {
                    try
                    {
                        return c.Parse();
                    }
                    catch (EntryNotSupportedException exception)
                    {
                        // TODO: Notify admins?
                        Logger.LogError(exception,
                            "A boss config for {MapName} uses unsupported configuration; ignoring", mapName);
                        return null;
                    }
                })
                .Where(c => c is not null)
                .Cast<BossConfig>()
                .ToList();
        }
        else
        {
            Logger.LogInformation("Could not find {ConfigName}", configName);
        }
    }

    private void SetupBoss(CEntityInstance entity)
    {
        var targetName = entity.Entity?.Name;
        Logger.LogInformation("Trying to setup {TargetName} (#{EntityId}) as boss", targetName, entity.Index);

        if (targetName is null) return; // TODO: hammer IDs?

        var bossConfig = _activeBossConfigs.Find(c => c switch
        {
            BreakableBossConfig breakableBossConfig => targetName == breakableBossConfig.BreakableName,
            SimpleCounterBossConfig simpleCounterBossConfig => targetName == simpleCounterBossConfig.CounterName,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        });

        if (bossConfig == null) return;

        Boss boss = bossConfig switch
        {
            BreakableBossConfig breakableBossConfig => new BreakableBoss(this, entity.As<CBreakable>())
            {
                Name = breakableBossConfig.Name
            },
            SimpleCounterBossConfig simpleCounterBossConfig => new SimpleCounterBoss(this, entity.As<CMathCounter>())
            {
                Name = simpleCounterBossConfig.Name,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(bossConfig))
        };

        boss.Initialize();
        _bossManager.Register(boss);
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
        if (_bossManager.BossMessage is null) return;

        foreach (var player in _activePlayers)
        {
            player?.PrintToCenterHtml(_bossManager.BossMessage);
        }
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        Server.NextFrame(() => { SetupBoss(entity); });
    }

    public void OnConfigParsed(BossHudPluginConfig config)
    {
        Config = config;
    }

    public required BossHudPluginConfig Config { get; set; }

    public void UnregisterBoss(Boss boss)
    {
        _bossManager.Unregister(boss);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _bossManager.Dispose();
    }
}

public class BossHudPluginConfig : BasePluginConfig;