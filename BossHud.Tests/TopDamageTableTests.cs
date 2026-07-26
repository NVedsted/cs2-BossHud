using BossHud.Bosses;
using TUnit.Assertions.Enums;

namespace BossHud.Tests;

public class TopDamageTableTests
{
    private static readonly (string Name, int Damage, int Hits)[] Entries =
    [
        ("Nick", 1000, 135),
        ("Ann", 741, 79),
        ("Mike", 657, 55),
        ("Simon", 1, 1),
        ("Clare", 0, 0),
    ];

    [Test]
    public async Task PrintHitsBoss()
    {
        var actual = TopDamageTable.BuildTopDamageTable(BossType.Hits, "Scary Hit Boss", Entries);

        string[] expected =
        [
            "{yellow}TOP BOSS DAMAGE - Scary Hit Boss",
            "{gold}1. Nick {default}- {red}1000 HITS",
            "{grey}2. Ann {default}- {red}741 HITS",
            "{yellow}3. Mike {default}- {red}657 HITS",
            "{blue}4. Simon {default}- {red}1 HIT",
            "{blue}5. Clare {default}- {red}0 HITS",
        ];

        await Assert.That(actual).IsEquivalentTo(expected, CollectionOrdering.Matching);
    }

    [Test]
    public async Task PrintBossWithNoName()
    {
        var actual = TopDamageTable.BuildTopDamageTable(BossType.Hits, null, Entries)
            .First();

        var expected = "{yellow}TOP BOSS DAMAGE";

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task PrintHpBoss()
    {
        var actual = TopDamageTable.BuildTopDamageTable(BossType.Hp, "Scary HP Boss", Entries);

        string[] expected =
        [
            "{yellow}TOP BOSS DAMAGE - Scary HP Boss",
            "{gold}1. Nick {default}- {red}1000 DMG {olive}(135 HITS)",
            "{grey}2. Ann {default}- {red}741 DMG {olive}(79 HITS)",
            "{yellow}3. Mike {default}- {red}657 DMG {olive}(55 HITS)",
            "{blue}4. Simon {default}- {red}1 DMG {olive}(1 HIT)",
            "{blue}5. Clare {default}- {red}0 DMG {olive}(0 HITS)",
        ];

        await Assert.That(actual).IsEquivalentTo(expected, CollectionOrdering.Matching);
    }
}