using System.Text;

namespace BossHud.Rendering;

// TODO: Show HITS VS HP
public static class Hud
{
    public record BossData(string? Name, int CurrentHealth, int MaxHealth, bool MinorHud);

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
            var color = Gradient.GetHpColor(hpPercentage);
            builder.Append(
                $"<font class=\"fontSize-l\" color=\"{color}\">{bossData.CurrentHealth}</font> ({hpPercentage * 100.0:N0}%)");
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

        var color = Gradient.GetHpColor(hpPercentage);
        builder.Append(
            $"<font class=\"fontSize-l\" color=\"{color}\">{bossData.CurrentHealth}</font> ({hpPercentage * 100.0:N0}%)<br><font color=\"{color}\">");

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
}