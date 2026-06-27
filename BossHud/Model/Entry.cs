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

    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("breakable")] public string? Breakable { get; init; }

    [JsonPropertyName("counter")] public string? Counter { get; init; }

    [JsonPropertyName("iterator")]
    [Unsupported]
    public string? Iterator { get; init; }

    [JsonPropertyName("backup")]
    [Unsupported]
    public string? Backup { get; init; }

    [JsonPropertyName("trigger")]
    [Unsupported]
    public OutputDefinitionWithDelay? Trigger { get; init; }

    [JsonPropertyName("showtrigger")]
    [Unsupported]
    public OutputDefinitionWithDelay? ShowTrigger { get; init; }

    [JsonPropertyName("killtrigger")]
    [Unsupported]
    public OutputDefinitionWithDelay? KillTrigger { get; init; }

    [JsonPropertyName("hurttrigger")]
    [Unsupported]
    public OutputDefinition? HurtTrigger { get; init; }

    public BossConfig Parse()
    {
        var unsupportedPropsSet = UnsupportedProps.Where(p => p.GetValue(this) != null)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
            .ToArray();

        if (unsupportedPropsSet.Length != 0)
        {
            throw new EntryNotSupportedException(
                $"Config contained the following unsupported properties: {string.Join(", ", unsupportedPropsSet)}");
        }

        if (Breakable is not null && Counter is not null)
        {
            throw new EntryNotSupportedException("Breakable and counter cannot both be set");
        }

        if (Breakable != null)
        {
            if (Breakable.StartsWith('#'))
            {
                throw new EntryNotSupportedException("Hammer ID not supported in breakable");
            }

            return new BreakableBossConfig
            {
                BreakableName = Breakable,
                Name = Name,
            };
        }

        Debug.Assert(Counter is not null);

        if (Counter.StartsWith('#'))
        {
            throw new EntryNotSupportedException("Hammer ID not supported in counter");
        }

        return new SimpleCounterBossConfig
        {
            Name =  Name,
            CounterName = Counter,
        };
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class UnsupportedAttribute : Attribute;

public class EntryNotSupportedException(string message) : Exception(message);