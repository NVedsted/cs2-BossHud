using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using BossHud.BossConfigs;
using BossHud.EntityIdentifiers;

namespace BossHud.Model;

public record JsonEntry
{
    private static readonly PropertyInfo[] UnsupportedProps = typeof(JsonEntry)
        .GetProperties()
        .Where(p => p.GetCustomAttribute<UnsupportedAttribute>() != null)
        .ToArray();

    /// <summary>
    /// Name of boss that appears in hud
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Targetname/Hammerid of breakable
    /// </summary>
    [JsonPropertyName("breakable")]
    public string? Breakable { get; init; }

    /// <summary>
    /// Targetname/Hammerid of counter
    /// </summary>
    [JsonPropertyName("counter")]
    public string? Counter { get; init; }

    /// <summary>
    /// Targetname/Hammerid of hp iterator (segments)
    /// </summary>
    [JsonPropertyName("iterator")]
    public string? Iterator { get; init; }

    /// <summary>
    /// Targetname/Hammerid of hp backup
    /// </summary>
    [JsonPropertyName("backup")]
    public string? Backup { get; init; }

    /// <summary>
    /// Specifies the event that triggers the boss
    /// </summary>
    [JsonPropertyName("trigger")]
    public JsonOutputDefinitionWithDelay? Trigger { get; init; }

    /// <summary>
    /// Specifies event that starts displaying boss health
    /// </summary>
    [JsonPropertyName("showtrigger")]
    [Unsupported]
    public JsonOutputDefinitionWithDelay? ShowTrigger { get; init; }

    /// <summary>
    /// Specifies event that force kills the boss
    /// </summary>
    [JsonPropertyName("killtrigger")]
    [Unsupported]
    public JsonOutputDefinitionWithDelay? KillTrigger { get; init; }

    /// <summary>
    /// Specifies event that is considered as damaging the boss
    /// </summary>
    [JsonPropertyName("hurttrigger")]
    [Unsupported]
    public JsonOutputDefinition? HurtTrigger { get; init; }

    /// <summary>
    /// Whether counter should be reversed
    /// </summary>
    [JsonPropertyName("reversecounter")]
    [Unsupported]
    public bool? ReverseCounter { get; init; }

    /// <summary>
    /// Whether iterator should be reversed
    /// </summary>
    [JsonPropertyName("reverseiterator")]
    [Unsupported]
    public bool? ReverseIterator { get; init; }

    [JsonPropertyName("hitmarkeronly")]
    [Unsupported]
    public bool? HitmarkerOnly { get; init; }

    /// <summary>
    /// Whether boss should be displayed as no-bar hud variant
    /// </summary>
    [JsonPropertyName("minorhud")]
    [Unsupported]
    public bool? MinorHud { get; init; }

    /// <summary>
    /// Whether boss can be triggered multiple times (multiple instances)
    /// </summary>
    [JsonPropertyName("multitrigger")]
    public bool? MultiTrigger { get; init; }

    /// <summary>
    /// Whether boss is templated and has name fixup
    /// </summary>
    [JsonPropertyName("templated")]
    public bool? Templated { get; init; }

    /// <summary>
    /// Whether top boss damage should be displayed after boss death
    /// </summary>
    [JsonPropertyName("showbeaten")]
    public bool? ShowBeaten { get; init; }

    /// <summary>
    /// Specify time before boss health is hidden after taking no damage
    /// </summary>
    [JsonPropertyName("timeout")]
    [Unsupported]
    public float? Timeout { get; init; }

    /// <summary>
    /// Specify amount of health to ADD to displayed health (negative to subtract)
    /// </summary>
    [JsonPropertyName("offset")]
    [Unsupported]
    public float? Offset { get; init; }

    /// <summary>
    /// Specify amount of iterator segments to ADD to displayed health (negative to subtract)
    /// </summary>
    [JsonPropertyName("offsetiterator")]
    [Unsupported]
    public float? OffsetIterator { get; init; }

    /// <summary>
    /// If the boss has more than this HP, it will not start showing on the HUD (0.0 = no limit)
    /// </summary>
    [JsonPropertyName("maxhp")]
    [Unsupported]
    public float? MaxHp { get; init; }

    public BossConfig Parse()
    {
        try
        {
            var unsupportedPropsSet = UnsupportedProps
                .Where(p => p.GetValue(this) != null)
                .Select(p =>
                    p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
                .ToArray();

            if (unsupportedPropsSet.Length != 0)
            {
                throw new EntryPropertiesNotSupported(unsupportedPropsSet);
            }

            var config = ParseBaseConfig();

            config.Name = Name;

            if (MultiTrigger.HasValue) config.MultiTrigger = MultiTrigger.Value;

            if (Templated.HasValue) config.Templated = Templated.Value;

            if (ShowBeaten.HasValue) config.ShowBeaten = ShowBeaten.Value;

            if (Trigger is not null) config.Trigger = Trigger.Parse(Templated ?? false);

            return config;
        }
        catch (InvalidOperationException exception)
        {
            throw new BossConfigException(Name, exception);
        }
    }

    private BossConfig ParseBaseConfig()
    {
        var templated = Templated ?? false;

        if (Breakable is not null && Counter is not null)
        {
            throw new InvalidOperationException("Breakable and counter cannot both be set");
        }

        if (Breakable != null)
        {
            if (Iterator is not null)
            {
                throw new BreakableWithIteratorNotSupportedException("Breakable with iterator is not supported");
            }

            if (Backup is not null)
            {
                throw new InvalidOperationException("Breakable and backup cannot both be set");
            }

            return new BreakableBossConfig { MainIdentifier = EntityIdentifier.Parse(Breakable).Validate(templated) };
        }

        Debug.Assert(Counter is not null);

        return (Iterator, Backup) switch
        {
            (null, null) => new SimpleCounterBossConfig
            {
                MainIdentifier = EntityIdentifier.Parse(Counter).Validate(templated),
            },
            (not null, null) => new SegmentedCounterBossConfig
            {
                MainIdentifier = EntityIdentifier.Parse(Counter).Validate(templated),
                IteratorIdentifier = EntityIdentifier.Parse(Iterator).Validate(templated),
            },
            (not null, not null) => new SegmentedWithBackupCounterBossConfig
            {
                MainIdentifier = EntityIdentifier.Parse(Counter).Validate(templated),
                IteratorIdentifier = EntityIdentifier.Parse(Iterator).Validate(templated),
                BackupIdentifier = EntityIdentifier.Parse(Backup).Validate(templated),
            },
            (null, not null) => throw new InvalidOperationException("Backup cannot be set without iterator"),
        };
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class UnsupportedAttribute : Attribute;

public abstract class EntryNotSupportedException(string message) : InvalidOperationException(message);

public sealed class BreakableWithIteratorNotSupportedException(string message) : EntryNotSupportedException(message);

public sealed class EntryPropertiesNotSupported(string[] properties)
    : EntryNotSupportedException(
        $"Config contained the following unsupported properties: {string.Join(", ", properties)}")
{
    public IEnumerable<string> Properties => properties;
}

public sealed class BossConfigException(string? bossName, Exception innerException)
    : Exception($"Configuration of {bossName ?? "<no-name>"} is not valid", innerException)
{
    public string? BossName => bossName;

    public ConfigState ConfigState { get; } = innerException switch
    {
        EntryNotSupportedException => ConfigState.Unsupported,
        _ => ConfigState.Broken,
    };
}