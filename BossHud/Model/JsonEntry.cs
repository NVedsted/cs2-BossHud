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
    public JsonOutputDefinitionWithDelay? ShowTrigger { get; init; }

    /// <summary>
    /// Specifies event that force kills the boss
    /// </summary>
    [JsonPropertyName("killtrigger")]
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
    public bool? ReverseCounter { get; init; }

    /// <summary>
    /// Whether iterator should be reversed
    /// </summary>
    [JsonPropertyName("reverseiterator")]
    public bool? ReverseIterator { get; init; }

    [JsonPropertyName("hitmarkeronly")]
    public bool? HitmarkerOnly { get; init; } // TODO: remember to validate ShowTrigger

    /// <summary>
    /// Whether boss should be displayed as no-bar hud variant
    /// </summary>
    [JsonPropertyName("minorhud")]
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
    public float? Timeout { get; init; }

    /// <summary>
    /// Specify amount of health to ADD to displayed health (negative to subtract)
    /// </summary>
    [JsonPropertyName("offset")]
    public float? Offset { get; init; }

    /// <summary>
    /// Specify amount of iterator segments to ADD to displayed health (negative to subtract)
    /// </summary>
    [JsonPropertyName("offsetiterator")]
    public float? IteratorOffset { get; init; }

    /// <summary>
    /// If the boss has more than this HP, it will not start showing on the HUD (0.0 = no limit)
    /// </summary>
    [JsonPropertyName("maxhp")]
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
            if (HitmarkerOnly.HasValue) config.HitMarkerOnly = HitmarkerOnly.Value;
            if (MaxHp > 0.0) config.MaxHp = (int)MaxHp.Value;
            if (Timeout > 0.0) config.Timeout = Timeout.Value;
            if (Offset.HasValue) config.Offset = (int)Offset.Value;
            if (MinorHud.HasValue) config.MinorHud = MinorHud.Value;

            if (Trigger is not null) config.Trigger = Trigger.Parse(Templated ?? false);
            if (KillTrigger is not null) config.KillTrigger = KillTrigger.Parse(false);
            if (ShowTrigger is not null) config.ShowTrigger = ShowTrigger.Parse(false);

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
            var breakableIdentifier = EntityIdentifier.Parse(Breakable).Validate(templated);

            if (Iterator is not null)
            {
                var config = new BreakableWithIteratorBossConfig
                {
                    MainIdentifier = breakableIdentifier,
                    IteratorIdentifier = EntityIdentifier.Parse(Iterator).Validate(templated)
                };

                if (IteratorOffset.HasValue) config.IteratorOffset = (int)IteratorOffset.Value;
                if (ReverseIterator.HasValue) config.ReverseIterator = ReverseIterator.Value;

                return config;
            }

            return new BreakableBossConfig { MainIdentifier = EntityIdentifier.Parse(Breakable).Validate(templated) };
        }

        Debug.Assert(Counter is not null);

        var counterIdentifier = EntityIdentifier.Parse(Counter).Validate(templated);

        BaseCounterBossConfig counterConfig = (Iterator, Backup) switch
        {
            (null, null) => new SimpleCounterBossConfig { MainIdentifier = counterIdentifier },
            (not null, null) => new SegmentedCounterBossConfig
            {
                MainIdentifier = counterIdentifier,
                IteratorIdentifier = EntityIdentifier.Parse(Iterator).Validate(templated),
            },
            (not null, not null) => new SegmentedWithBackupCounterBossConfig
            {
                MainIdentifier = counterIdentifier,
                IteratorIdentifier = EntityIdentifier.Parse(Iterator).Validate(templated),
                BackupIdentifier = EntityIdentifier.Parse(Backup).Validate(templated),
            },
            (null, not null) => throw new InvalidOperationException("Backup cannot be set without iterator"),
        };

        if (ReverseCounter.HasValue) counterConfig.ReverseCounter = ReverseCounter.Value;

        if (counterConfig is SegmentedCounterBossConfig segmentedConfig)
        {
            if (IteratorOffset.HasValue && IteratorOffset != 0.0)
                segmentedConfig.IteratorOffset = (int)IteratorOffset.Value;
        }

        if (counterConfig is SegmentedWithBackupCounterBossConfig withBackupConfig)
        {
            // TODO: Refactor unused detection
            if (ReverseIterator.HasValue) withBackupConfig.ReverseIterator = ReverseIterator.Value;
        }

        return counterConfig;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class UnsupportedAttribute : Attribute;

public abstract class EntryNotSupportedException(string message) : InvalidOperationException(message);

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