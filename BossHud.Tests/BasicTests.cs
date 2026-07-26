using System.Text.Json;
using System.Text.Json.Serialization;
using BossHud.Model;

namespace BossHud.Tests;

public class BasicTests
{
    [Test]
    public async Task Hm()
    {
        var success = 0;
        var total = 0;

        var unsupportedStats = new Dictionary<string, long>();

        foreach (var fileName in Directory.EnumerateFiles("../../../../configs"))
        {
            await using var stream = File.OpenRead(fileName);
            var entries = await JsonSerializer.DeserializeAsync<Entry[]>(stream, new JsonSerializerOptions
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
                    catch (EntryPropertiesNotSupported exception)
                    {
                        foreach (var property in exception.Properties)
                        {
                            unsupportedStats[property] = unsupportedStats.GetValueOrDefault(property) + 1;
                        }

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

        await Assert.That(success).IsEqualTo(352);
    }
}