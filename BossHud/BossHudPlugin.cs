using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

namespace BossHud;

[MinimumApiVersion(367)]
public class BossHudPlugin : BasePlugin
{
    public override string ModuleName => "BossHud";
    public override string ModuleVersion => "Nicklas Vedsted";

    private readonly List<Boss> _initializedBosses = [];

    private readonly CCSPlayerController?[] _activePlayers = new CCSPlayerController?[64];

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        RegisterListener<Listeners.OnTick>(OnTick);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(EventPlayerDisconnect);

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
        }
    }

    private void SetupBoss(CEntityInstance entity)
    {
        Boss? boss = entity.DesignerName switch
        {
            "func_breakable" => new BreakableBoss(this, entity.As<CBreakable>()) { Name = entity.Entity!.Name },
            "math_counter" => new SimpleCounterBoss(this, entity.As<CMathCounter>()) { Name = entity.Entity!.Name },
            _ => null
        };

        if (boss == null) return;

        boss.Initialize();
        _initializedBosses.Add(boss);
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
        foreach (var player in _activePlayers)
        {
            if (player is null) continue;

            _initializedBosses.RemoveAll(b => b.Disposed);

            var msg = string.Join('\n', _initializedBosses
                .Where(b => b is { Disposed: false, Active: true })
                .Select(b => $"{b.Name}: {b.CurrentHealth} / {b.MaxHealth} ({(float)b.CurrentHealth / b.MaxHealth * 100.0f:F1}%)"));

            if (msg != "")
            {
                player.PrintToCenterHtml(msg);
            }
        }
    }

    private void OnEntityCreated(CEntityInstance entity)
    {
        Server.NextFrame(() =>
        {
            SetupBoss(entity);
        });
    }
}