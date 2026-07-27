using System.Text.Json;
using System.Text.Json.Serialization;
using BossHud.Model;

namespace BossHud.Tests;

public class ConfigTests
{
    [Test]
    public async Task ValidateConfigs()
    {
        var success = 0;
        var total = 0;

        var unsupportedStats = new Dictionary<string, long>();

        foreach (var fileName in Directory.EnumerateFiles("../../../../gfl-configs/bosshud")
                     .Where(fileName => !fileName.EndsWith("template.jsonc")))
        {
            await using var stream = File.OpenRead(fileName);
            var entries = await JsonSerializer.DeserializeAsync<JsonEntry[]>(stream, new JsonSerializerOptions
            {
                TypeInfoResolver = BossHudJsonContext.Default,
                ReadCommentHandling = JsonCommentHandling.Skip,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            });

            if (entries is null) throw new InvalidOperationException($"{fileName} could not be parsed");

            total += entries.Length;

            success += entries
                .Select(e =>
                {
                    try
                    {
                        return e.Parse();
                    }
                    catch (BossConfigException exception)
                    {
                        if (exception.InnerException is EntryPropertiesNotSupportedException propertiesNotSupported)
                        {
                            foreach (var property in propertiesNotSupported.Properties)
                            {
                                unsupportedStats[property] = unsupportedStats.GetValueOrDefault(property) + 1;
                            }
                        }

                        Console.WriteLine(exception);
                        Console.WriteLine(fileName);

                        return null;
                    }
                })
                .Count(e => e is not null);
        }

        Console.WriteLine($"Success: {success}, Total: {total} ({success / (double)total * 100.0:F1}%)");

        Console.WriteLine("Unsupported stats:");
        foreach (var keyValuePair in unsupportedStats.OrderByDescending(kvp => kvp.Value))
        {
            Console.WriteLine($"\t{keyValuePair.Key}: {keyValuePair.Value}");
        }

        await Assert.That(success).IsEqualTo(848);
    }
}