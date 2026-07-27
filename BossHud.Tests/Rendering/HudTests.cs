using BossHud.Rendering;

namespace BossHud.Tests.Rendering;

public class HudTests
{
    [Test]
    public async Task NoBossReturnsNull()
    {
        Hud.BossData[] bosses = [];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud).IsNull();
    }

    [Test]
    public async Task SingleNonMinor()
    {
        Hud.BossData[] bosses = [new("Scary Boss", 30, 100)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Scary Boss<br><font class="fontSize-l" color="#FF9900">30</font> (30%)<br><font color="#FF9900">██████</font><font color="gray">██████████████</font>""");
    }

    [Test]
    public async Task TwoBosses()
    {
        Hud.BossData[] bosses =
        [
            new("Higher Boss", 950, 1000),
            new("Scary Boss", 30, 100),
        ];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Higher Boss: <font class="fontSize-l" color="#1AFF00">950</font> (95%)<br>Scary Boss: <font class="fontSize-l" color="#FF9900">30</font> (30%)""");
    }
}