namespace BossHud.Model;

public sealed class EntryPropertiesNotSupportedException(string[] properties)
    : InvalidOperationException(
        $"Config contained the following unsupported properties: {string.Join(", ", properties)}")
{
    public IEnumerable<string> Properties => properties;
}