namespace BossHud.EntityIdentifiers;

public sealed class EntityNotFoundException(EntityIdentifier entityIdentifier)
    : Exception($"Could not find entity with identifier {entityIdentifier}");