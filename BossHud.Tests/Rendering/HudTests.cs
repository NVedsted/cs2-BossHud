using BossHud.Rendering;

namespace BossHud.Tests.Rendering;

public class HudTests
{
    [Test]
    public async Task Test()
    {
        var boss = new Hud.BossData("Scary Boss", 30, 100);

        var hud = Hud.BuildHud(boss);

        await Assert.That(hud)
            .IsEqualTo(
                "<font>Scary Boss</font><br><font class=\"fontSize-l\" color=\"#FF9900\">30</font> <font>(30%)</font><br><font color=\"#FF9900\">██████</font><font color=\"gray\">██████████████</font>");
    }
}