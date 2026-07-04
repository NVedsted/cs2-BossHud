using System.Text;
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

    private readonly BossManager _bossManager = new();

    private readonly CCSPlayerController?[] _activePlayers = new CCSPlayerController?[64];

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(EventPlayerDisconnect);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnded);

        SetupMessageUpdater();

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                _activePlayers[player.Slot] = player;
            }

            foreach (var entity in Utilities.GetAllEntities())
            {
                SetupBoss(entity);
            }

            OnMapStart(Server.MapName);
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

    private void SetupMessageUpdater()
    {
        var token = _cancellationTokenSource.Token;
        // Runs in worker thread
        Task.Run(async () =>
        {
            void UpdateMessage()
            {
                var activeBoss = _bossManager.ActiveBosses
                    .FirstOrDefault(b => b is
                        { Disposed: false, Active: true }); // TODO: Pick more intelligently or multi-show

                if (activeBoss is null)
                {
                    _bossMessage = null;
                    return;
                }

                var hpPercentage = activeBoss.CurrentHealth / (double)activeBoss.MaxHealth;
                var displaySegments = (int)Math.Round(hpPercentage * 20.0, MidpointRounding.AwayFromZero);
                var remaining = 20 - displaySegments;

                var builder = new StringBuilder();
                if (activeBoss.Name is not null)
                {
                    builder.Append($"<font>{activeBoss.Name}</font><br>");
                }

                builder.Append(
                    $"<font class=\"fontSize-l\" color=\"green\">{activeBoss.CurrentHealth}</font> <font>({hpPercentage * 100.0:N0}%)</font><br><font color=\"green\">");

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

                _bossMessage = builder.ToString();
            }

            Logger.LogInformation("Starting async boss message thread");
            while (!token.IsCancellationRequested)
            {
                // TODO: disable when no active bosses
                UpdateMessage();
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
            }

            Logger.LogInformation("Thread died");
        }, token);
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

    private string? _bossMessage;

    private void OnTick()
    {
        if (_bossMessage is null) return;

        foreach (var player in _activePlayers)
        {
            if (player is null) continue;

            player.PrintToCenterHtml(_bossMessage);
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
}

public class BossHudPluginConfig : BasePluginConfig;