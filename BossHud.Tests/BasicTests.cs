using System.Text.Json;
using BossHud.Model;

namespace BossHud.Tests;

public class BasicTests
{
    [Test]
    public async Task Hm()
    {

        foreach (var fileName in Directory.EnumerateFiles("C:\\Users\\nickl\\RiderProjects\\BossHud\\configs"))
        {
            await using var stream = File.OpenRead(fileName);
            var hm = await JsonSerializer.DeserializeAsync<Entry[]>(stream, new JsonSerializerOptions
            {
                TypeInfoResolver = BossHudJsonContext.Default,
                ReadCommentHandling = JsonCommentHandling.Skip,
            });

            if (hm is null) throw new InvalidOperationException($"{fileName} could not be parsed");

            Console.WriteLine(fileName);
            foreach (var entry in hm)
            {
                Console.WriteLine($"\t{entry}");
            }
        }
    }
}