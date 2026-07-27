namespace BossHud.Model;

public sealed class BossConfigException(string? bossName, Exception innerException)
    : Exception($"Configuration of {bossName ?? "<no-name>"} is not valid", innerException)
{
    public string? BossName => bossName;

    public ConfigState ConfigState { get; } = innerException switch
    {
        EntryPropertiesNotSupportedException => ConfigState.Unsupported,
        _ => ConfigState.Broken,
    };
}