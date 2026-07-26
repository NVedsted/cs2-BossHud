using BossHud.Bosses;
using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API.Core;
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

    public required EntityIdentifier MainIdentifier { get; init; }

    public bool ShowBeaten { get; set; } = true;

    public bool MultiTrigger { get; set; } = false;

    public bool Templated { get; set; } = false;

    public bool HitMarkerOnly { get; set; } = false;

    public void SetupBoss(BossHudPlugin plugin, CEntityInstance caller)
    {
        // Mark immediately as seen to not spam timers.
        // TODO: can we avoid timer spam for delayed multi-triggers?
        if (!MultiTrigger && !plugin.BossManager.TryMarkBossConfigAsTriggered(this)) return;

        var templateId = Templated ? ExtractTemplateId(caller) : null;

        if (Trigger.Delay.HasValue)
        {
            plugin.AddRoundTimer(Trigger.Delay.Value, () => SetupBossInternal(plugin, templateId));
        }
        else
        {
            SetupBossInternal(plugin, templateId);
        }
    }

    private static string? ExtractTemplateId(CEntityInstance entity)
    {
        var name = entity.Entity?.Name;

        if (name == null) return null;

        var separator = name.LastIndexOf('_');

        if (separator == -1) return null;

        return name[(separator + 1)..];
    }

    private void SetupBossInternal(BossHudPlugin plugin, string? templateId)
    {
        var mainEntity = MainIdentifier.WithTemplateId(templateId).Find()!; // TODO: not found?

        // Ignore if entity is already part of a boss.
        if (plugin.BossManager.IsBossEntity(mainEntity)) return;

        if (templateId is null)
        {
            plugin.Logger.LogInformation("Setting up boss {BossName}", Name ?? "<no-name>");
        }
        else
        {
            plugin.Logger.LogInformation("Setting up templated boss {BossName} (Template ID: {TemplateId})",
                Name ?? "<no-name>", templateId);
        }

        var boss = CreateBoss(new CreateBossContext
        {
            Config = this,
            Plugin = plugin,
            MainEntity = mainEntity,
            TemplateId = templateId,
        });

        boss.Initialize();
        plugin.BossManager.Register(boss);
    }

    protected abstract Boss CreateBoss(CreateBossContext context);
}