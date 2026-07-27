namespace BossHud.Rendering;

public static class Gradient
{
    /// <summary>
    /// Computes 3-point gradient (red-yellow-green) for HP bars.
    /// </summary>
    /// <param name="hpPercentage">The percentage (between 0.0 and 1.0)</param>
    /// <returns>The HP color as a CSS color (#RRGGBB)</returns>
    public static string GetHpColor(double hpPercentage)
    {
        if (hpPercentage < 0.5)
        {
            var t = hpPercentage * 2;
            var g = (byte)(t * 255);
            return $"#FF{g:X2}00";
        }
        else
        {
            var t = (hpPercentage - 0.5) * 2;
            var r = 255 - (byte)(t * 255);
            return $"#{r:X2}FF00";
        }
    }
}