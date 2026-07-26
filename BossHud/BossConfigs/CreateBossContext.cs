using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs;

public class CreateBossContext
{
    public required BossConfig Config { get; init; }

    public required BossHudPlugin Plugin { get; init; }

    public required CEntityInstance MainEntity { get; init; }

    public required string? TemplateId { get; init; }

    public T GetEntity<T>(EntityIdentifier entityIdentifier) where T : NativeObject =>
        entityIdentifier.WithTemplateId(TemplateId).Find()?.As<T>()!; // TODO: What if not found?
}