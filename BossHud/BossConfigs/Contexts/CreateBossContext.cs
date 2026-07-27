using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs.Contexts;

public sealed class CreateBossContext
{
    public required BossHudPlugin Plugin { get; init; }

    public required CEntityInstance MainEntity { get; init; }

    public required string? TemplateId { get; init; }

    public T GetEntity<T>(EntityIdentifier entityIdentifier) where T : NativeObject =>
        entityIdentifier.WithTemplateId(TemplateId).Get().As<T>();

    public ICreateBossContextWithConfig<TConfig> WithConfig<TConfig>(TConfig config) =>
        new CreateBossContextWithConfig<TConfig>(this, config);
}