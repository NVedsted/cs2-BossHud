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

    public override string ToString() => _kind switch
    {
        Kind.TargetName => _targetName,
        Kind.TemplatedTargetName => $"{_targetName}_*",
        Kind.HammerId => $"#{_targetName}",
        _ => throw new ArgumentOutOfRangeException()
    };

    private readonly string _targetName;

    private EntityIdentifier(Kind kind, string targetName)
    {
        _kind = kind;
        _targetName = targetName;
    }

    public static EntityIdentifier Parse(string identifier) => identifier.StartsWith('#')
        ? new EntityIdentifier(Kind.HammerId, identifier[1..])
        : new EntityIdentifier(Kind.TargetName, identifier);

    public EntityIdentifier AsTemplated(bool templated = true) => (templated, _kind) switch
    {
        (false, _) or (true, Kind.TemplatedTargetName) => this,
        (true, Kind.TargetName) => new EntityIdentifier(Kind.TemplatedTargetName, _targetName),
        (true, Kind.HammerId) => throw new InvalidOperationException("HammerID cannot be templated"),
        (true, _) => throw new ArgumentOutOfRangeException()
    };

    public EntityIdentifier WithTemplateId(string? templateId) => (templateId, _kind) switch
    {
        (null, _) => this,
        (var id, Kind.TargetName or Kind.TemplatedTargetName) =>
            new EntityIdentifier(Kind.TargetName, $"{_targetName}_{id}"),
        (_, Kind.HammerId) => this,
        _ => throw new ArgumentOutOfRangeException()
    };

    public EntityIdentifier Validate(bool mustBeAbleToBeTemplated) =>
        mustBeAbleToBeTemplated && _kind is Kind.HammerId
            ? throw new InvalidOperationException("HammerID cannot be templated")
            : this;

    public CEntityInstance Get() =>
        Utilities.GetAllEntities().FirstOrDefault(Matches) ?? throw new EntityNotFoundException(this);

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