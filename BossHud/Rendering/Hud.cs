using System.Text;

namespace BossHud.Rendering;

public static class Hud
{
    public record BossData(string? Name, int CurrentHealth, int MaxHealth);

    public static string BuildHud(BossData  bossData)
    {
        // TODO: cap at 100%
        var hpPercentage = bossData.CurrentHealth / (double)bossData.MaxHealth;
        var displaySegments = (int)Math.Round(hpPercentage * 20.0, MidpointRounding.AwayFromZero);
        var remaining = 20 - displaySegments;

        var builder = new StringBuilder();
        if (bossData.Name is not null)
        {
            builder.Append($"<font>{bossData.Name}</font><br>");
        }

        var color = Gradient.GetHpColor(hpPercentage);
        builder.Append(
            $"<font class=\"fontSize-l\" color=\"{color}\">{bossData.CurrentHealth}</font> <font>({hpPercentage * 100.0:N0}%)</font><br><font color=\"{color}\">");

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