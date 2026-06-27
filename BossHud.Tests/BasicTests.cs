using System.Text.Json;
using BossHud.Model;

namespace BossHud.Tests;

public class BasicTests
{
    [Test]
    public async Task Hm()
    {
        var success = 0;
        var total = 0;
        foreach (var fileName in Directory.EnumerateFiles("../../../../configs"))
        {
            await using var stream = File.OpenRead(fileName);
            var entries = await JsonSerializer.DeserializeAsync<Entry[]>(stream, new JsonSerializerOptions
            {
                TypeInfoResolver = BossHudJsonContext.Default,
                ReadCommentHandling = JsonCommentHandling.Skip,
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
                    catch
                    {
                        // ignored
                        return null;
                    }
                })
                .Count(e => e is not null);
        }

        Console.WriteLine($"Success: {success}, Total: {total} ({success / (double)total * 100.0:F1}%)");
    }
}