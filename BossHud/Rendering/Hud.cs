using System.Text;
using BossHud.Bosses;

namespace BossHud.Rendering;

public static class Hud
{
    public record BossData(string? Name, int CurrentHealth, int MaxHealth, bool MinorHud, BossType Type);

    public static string? BuildHud(IEnumerable<BossData> bosses)
    {
        using var enumerator = bosses.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            // No bosses were present.
            return null;
        }

        var first = enumerator.Current;

        if (!enumerator.MoveNext())
        {
            // Only one boss is present.
            if (first.MinorHud) return BuildMultiShow([first]);

            return BuildMajorHud(first);
        }

        var second = enumerator.Current;

        return BuildMultiShow(Rebuild(first, second, enumerator));

        IEnumerable<BossData> Rebuild(BossData x, BossData y, IEnumerator<BossData> rest)
        {
            yield return x;
            yield return y;
            while (rest.MoveNext())
            {
                yield return rest.Current;
            }
        }
    }

    private static string BuildMultiShow(IEnumerable<BossData> bosses)
    {
        var builder = new StringBuilder();
        var first = true;
        foreach (var bossData in bosses)
        {
            if (!first)
            {
                builder.Append("<br>");
            }

            first = false;

            if (bossData.Name is not null)
            {
                builder.Append($"{bossData.Name}: ");
            }

            var hpPercentage = bossData.CurrentHealth / (double)bossData.MaxHealth;
            builder.AppendHealth(bossData, hpPercentage, false);
            builder.Append($" ({hpPercentage * 100.0:N0}%)");
        }

        return builder.ToString();
    }

    private static string BuildMajorHud(BossData bossData)
    {
        var hpPercentage = bossData.CurrentHealth / (double)bossData.MaxHealth;
        var displaySegments = (int)Math.Round(hpPercentage * 20.0, MidpointRounding.AwayFromZero);
        var remaining = 20 - displaySegments;

        var builder = new StringBuilder();
        if (bossData.Name is not null)
        {
            builder.Append($"{bossData.Name}<br>");
        }
        else
        {
            // Hack to ensure HP bar is shown.
            // If the first thing is a `fontSize-l`-classed element, subsequent lines are hidden.
            // TODO: investigate further
            builder.Append('\u200B');
        }

        var color = Gradient.GetHpColor(hpPercentage);
        builder.AppendHealth(bossData, hpPercentage, true);
        builder.Append($"<br><font color=\"{color}\">");

        for (var i = 0; i < displaySegments; i++)
        {
            builder.Append('█');
        }

        builder.Append("</font><font color=\"gray\">");

        for (var i = 0; i < remaining; i++)
        {
            builder.Append('█');
        }

        builder.Append("</font>");

        return builder.ToString();
    }

    private static void AppendHealth(this StringBuilder builder, BossData bossData, double hpPercentage, bool major)
    {
        var color = Gradient.GetHpColor(hpPercentage);
        var unit = (bossData.Type, bossData.CurrentHealth) switch
        {
            (BossType.Hp, _) => "DMG",
            (BossType.Hits, 1) => "HIT",
            (BossType.Hits, _) => "HITS",
            _ => throw new InvalidOperationException()
        };
        builder.Append(
            $"<font {(major ? "class=\"fontSize-l\" " : "")}color=\"{color}\">{bossData.CurrentHealth} {unit}</font>");
    }
}