using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs.Contexts;

public interface ICreateBossContextWithConfig<out TConfig>
{
    public TConfig Config { get; }

    public BossHudPlugin Plugin { get; }

    public CEntityInstance MainEntity { get; }

    public string? TemplateId { get; }

    public T GetEntity<T>(EntityIdentifier entityIdentifier) where T : NativeObject =>
        entityIdentifier.WithTemplateId(TemplateId).Find()?.As<T>()!; // TODO: What if not found?
}