using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using BossHud.BossConfigs;

namespace BossHud.Model;

public record Entry
{
    private static readonly PropertyInfo[] UnsupportedProps = typeof(Entry)
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
    public OutputDefinitionWithDelay? Trigger { get; init; }

    /// <summary>
    /// Specifies event that starts displaying boss health
    /// </summary>
    [JsonPropertyName("showtrigger")]
    [Unsupported]
    public OutputDefinitionWithDelay? ShowTrigger { get; init; }

    /// <summary>
    /// Specifies event that force kills the boss
    /// </summary>
    [JsonPropertyName("killtrigger")]
    [Unsupported]
    public OutputDefinitionWithDelay? KillTrigger { get; init; }

    /// <summary>
    /// Specifies event that is considered as damaging the boss
    /// </summary>
    [JsonPropertyName("hurttrigger")]
    [Unsupported]
    public OutputDefinition? HurtTrigger { get; init; }

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
    /// Whether boss should should be displayed as no-bar hud variant
    /// </summary>
    [JsonPropertyName("minorhud")]
    [Unsupported]
    public bool? MinorHud { get; init; }

    /// <summary>
    /// Whether boss can be triggered multiple times (multiple instances)
    /// </summary>
    [JsonPropertyName("multitrigger")]
    [Unsupported]
    public bool? MultiTrigger { get; init; }

    /// <summary>
    /// Whether boss is templated and has name fixup
    /// </summary>
    [JsonPropertyName("templated")]
    [Unsupported]
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

        if (Trigger is not null) config.Trigger = Trigger;

        if (ShowBeaten is not null) config.ShowBeaten =  ShowBeaten.Value;

        return config;
    }

    private BossConfig ParseBaseConfig()
    {
        if (Breakable is not null && Counter is not null)
        {
            throw new InvalidOperationException("Breakable and counter cannot both be set"); // TODO: Catch
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

            if (Breakable.StartsWith('#'))
            {
                throw new HammerIdNotSupportedException(
                    "Hammer ID not supported in breakable");
            }

            return new BreakableBossConfig { BreakableName = Breakable };
        }

        Debug.Assert(Counter is not null);

        if (Counter.StartsWith('#'))
        {
            throw new HammerIdNotSupportedException(
                "Hammer ID not supported in counter");
        }

        return (Iterator, Backup) switch
        {
            (null, null) => new SimpleCounterBossConfig { CounterName = Counter },
            (not null, null) => new SegmentedCounterBossConfig { CounterName = Counter, IteratorName = Iterator },
            (not null, not null) => new SegmentedWithBackupCounterBossConfig
            {
                CounterName = Counter,
                IteratorName = Iterator,
                BackupName = Backup
            },
            (null, not null) => throw new InvalidOperationException(
                "Backup cannot be set without iterator"), // TODO: name of boss
        };
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class UnsupportedAttribute : Attribute;

public abstract class EntryNotSupportedException(string message) : Exception(message);

public class HammerIdNotSupportedException(string message) : EntryNotSupportedException(message);

public class BreakableWithIteratorNotSupportedException(string message) : EntryNotSupportedException(message);

public class EntryPropertiesNotSupported(string[] properties)
    : EntryNotSupportedException(
        $"Config contained the following unsupported properties: {string.Join(", ", properties)}")
{
    public IEnumerable<string> Properties => properties;
}