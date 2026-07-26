using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace BossHud.EntityIdentifiers;

public class EntityIdentifier
{
    private enum Kind
    {
        TargetName,
        TemplatedTargetName,
        HammerId,
    }

    private readonly Kind _kind;
    private readonly string _targetName;

    private EntityIdentifier(Kind kind, string targetName)
    {
        _kind = kind;
        _targetName = targetName;
    }

    public static EntityIdentifier Parse(string identifier, bool templated) =>
        (identifier.StartsWith('#'), templated) switch
        {
            (true, false) => new EntityIdentifier(Kind.HammerId, identifier[1..]),
            (true, true) => throw new InvalidOperationException("HammerID cannot be templated"), // TODO: Catch
            (false, false) => new EntityIdentifier(Kind.TargetName, identifier),
            (false, true) => new EntityIdentifier(Kind.TemplatedTargetName, identifier),
        };


    public CEntityInstance? Get() => Utilities.GetAllEntities().FirstOrDefault(Matches);

    public bool Matches(CEntityInstance other)
    {
        return _kind switch
        {
            Kind.TargetName => other.Entity?.Name == _targetName,
            Kind.TemplatedTargetName => other.Entity?.Name == _targetName ||
                                        (other.Entity?.Name?.StartsWith(_targetName) ?? false), // Name can be null!
            Kind.HammerId => other.As<CBaseEntity>().UniqueHammerID == _targetName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}