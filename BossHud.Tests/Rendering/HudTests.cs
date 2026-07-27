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
    public async Task SingleMajor()
    {
        Hud.BossData[] bosses = [new("Scary Boss", 30, 100, false)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Scary Boss<br><font class="fontSize-l" color="#FF9900">30</font> (30%)<br><font color="#FF9900">██████</font><font color="gray">██████████████</font>""");
    }

    [Test]
    public async Task SingleMajorNoName()
    {
        Hud.BossData[] bosses = [new(null, 30, 100, false)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """<font class="fontSize-l" color="#FF9900">30</font> (30%)<br><font color="#FF9900">██████</font><font color="gray">██████████████</font>""");
    }

    [Test]
    public async Task SingleMinor()
    {
        Hud.BossData[] bosses = [new("Scary Boss", 30, 100, true)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud).IsEqualTo("""Scary Boss: <font class="fontSize-l" color="#FF9900">30</font> (30%)""");
    }

    [Test]
    public async Task SingleMinorNoName()
    {
        Hud.BossData[] bosses = [new(null, 30, 100, true)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud).IsEqualTo("""<font class="fontSize-l" color="#FF9900">30</font> (30%)""");
    }

    [Test]
    public async Task TwoBosses()
    {
        Hud.BossData[] bosses =
        [
            new("Higher Boss", 950, 1000, false),
            new("Scary Boss", 30, 100, false),
        ];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Higher Boss: <font class="fontSize-l" color="#1AFF00">950</font> (95%)<br>Scary Boss: <font class="fontSize-l" color="#FF9900">30</font> (30%)""");
    }

    [Test]
    public async Task TwoBossesOneWithoutName()
    {
        Hud.BossData[] bosses =
        [
            new("Higher Boss", 950, 1000, false),
            new(null, 30, 100, false),
        ];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Higher Boss: <font class="fontSize-l" color="#1AFF00">950</font> (95%)<br><font class="fontSize-l" color="#FF9900">30</font> (30%)""");
    }
}