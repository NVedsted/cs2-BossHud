using BossHud.Bosses;
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
        Hud.BossData[] bosses = [new("Scary Boss", 30, 100, false, BossType.Hp)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Scary Boss<br><font class="fontSize-l" color="#FF9900">30 DMG</font><br><font color="#FF9900">██████</font><font color="gray">██████████████</font>""");
    }

    [Test]
    public async Task SingleHit()
    {
        Hud.BossData[] bosses = [new("Weak Boss", 1, 30, false, BossType.Hits)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Weak Boss<br><font class="fontSize-l" color="#FF1100">1 HIT</font><br><font color="#FF1100">█</font><font color="gray">███████████████████</font>""");
    }

    [Test]
    public async Task SingleMajorNoName()
    {
        Hud.BossData[] bosses = [new(null, 30, 100, false, BossType.Hp)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                "\u200B<font class=\"fontSize-l\" color=\"#FF9900\">30 DMG</font><br><font color=\"#FF9900\">██████</font><font color=\"gray\">██████████████</font>");
    }

    [Test]
    public async Task SingleMinor()
    {
        Hud.BossData[] bosses = [new("Scary Boss", 30, 100, true, BossType.Hp)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud).IsEqualTo("""Scary Boss: <font color="#FF9900">30 DMG</font> (30%)""");
    }

    [Test]
    public async Task SingleMinorNoName()
    {
        Hud.BossData[] bosses = [new(null, 30, 100, true, BossType.Hp)];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud).IsEqualTo("""<font color="#FF9900">30 DMG</font> (30%)""");
    }

    [Test]
    public async Task TwoBosses()
    {
        Hud.BossData[] bosses =
        [
            new("Higher Boss", 950, 1000, false, BossType.Hp),
            new("Scary Boss", 30, 100, false, BossType.Hits),
        ];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Higher Boss: <font color="#1AFF00">950 DMG</font> (95%)<br>Scary Boss: <font color="#FF9900">30 HITS</font> (30%)""");
    }

    [Test]
    public async Task TwoBossesOneWithoutName()
    {
        Hud.BossData[] bosses =
        [
            new("Higher Boss", 950, 1000, false, BossType.Hits),
            new(null, 30, 100, false, BossType.Hp),
        ];

        var hud = Hud.BuildHud(bosses);

        await Assert.That(hud)
            .IsEqualTo(
                """Higher Boss: <font color="#1AFF00">950 HITS</font> (95%)<br><font color="#FF9900">30 DMG</font> (30%)""");
    }
}