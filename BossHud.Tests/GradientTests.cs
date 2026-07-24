namespace BossHud.Tests;

public class GradientTests
{
    [Test]
    public async Task LimitTests()
    {
        await Assert.That(Gradient.GetHpColor(0.0)).EqualTo("#FF0000");
        await Assert.That(Gradient.GetHpColor(0.5)).EqualTo("#FFFF00");
        await Assert.That(Gradient.GetHpColor(1.0)).EqualTo("#00FF00");
    }
}