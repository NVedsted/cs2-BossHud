using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.EntityIdentifiers;

public class EntityIdentifier
{
    private enum Kind
    {
        TargetName,
        HammerId,
    }

    private readonly Kind _kind;
    private readonly string _targetName;

    private EntityIdentifier(Kind kind, string targetName)
    {
        _kind = kind;
        _targetName = targetName;
    }

    public static EntityIdentifier Parse(string identifier) => identifier.StartsWith('#')
        ? new EntityIdentifier(Kind.HammerId, identifier[1..])
        : new EntityIdentifier(Kind.TargetName, identifier);

    public CEntityInstance? Get() => Utilities.GetAllEntities().FirstOrDefault(Matches);

    public bool Matches(CEntityInstance other)
    {
        return _kind switch
        {
            Kind.TargetName => other.Entity?.Name == _targetName,
            Kind.HammerId => other.As<CBaseEntity>().UniqueHammerID == _targetName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}