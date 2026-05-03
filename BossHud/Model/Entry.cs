using System.Text.Json.Serialization;

namespace BossHud.Model;

public record Entry
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("breakable")]
    public string? Breakable { get; init; }

    [JsonPropertyName("counter")]
    public string? Counter { get; init; }

    [JsonPropertyName("iterator")]
    public string? Iterator { get; init; }

    [JsonPropertyName("backup")]
    public string? Backup { get; init; }

    [JsonPropertyName("trigger")]
    public OutputDefinitionWithDelay? Trigger { get; init; }

    [JsonPropertyName("showtrigger")]
    public OutputDefinitionWithDelay? ShowTrigger { get; init; }

    [JsonPropertyName("killtrigger")]
    public OutputDefinitionWithDelay? KillTrigger { get; init; }

    [JsonPropertyName("hurttrigger")]
    public OutputDefinition? HurtTrigger { get; init; }
}