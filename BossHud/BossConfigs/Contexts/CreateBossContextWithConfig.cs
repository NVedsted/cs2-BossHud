using BossHud.EntityIdentifiers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.BossConfigs.Contexts;

public sealed class CreateBossContextWithConfig<TConfig>(CreateBossContext bossContext, TConfig config)
    : ICreateBossContextWithConfig<TConfig>
{
    public TConfig Config => config;
    public BossHudPlugin Plugin => bossContext.Plugin;
    public CEntityInstance MainEntity => bossContext.MainEntity;
    public string? TemplateId => bossContext.TemplateId;

    public T GetEntity<T>(EntityIdentifier entityIdentifier) where T : NativeObject =>
        bossContext.GetEntity<T>(entityIdentifier);
}