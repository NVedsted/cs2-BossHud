using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.EntityIdentifiers;

// TODO: Find more?
// TODO: Perf test and consider flatten?

class TargetNameEntityIdentifier(string targetName) : EntityIdentifier
{
    public override CEntityInstance? Get() => Utilities.GetAllEntities()
        .FirstOrDefault(e => e.Entity?.Name == targetName);

    public override bool Matches(CEntityInstance other) => other.Entity?.Name == targetName;
}

public abstract class EntityIdentifier
{
    public abstract CEntityInstance? Get();

    public abstract bool Matches(CEntityInstance other);
}