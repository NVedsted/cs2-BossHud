namespace BossHud.Bosses;

public class BossOrderComparer : IComparer<IBoss>
{
    public static BossOrderComparer Instance { get; } = new();

    private BossOrderComparer()
    {
    }

    public int Compare(IBoss? x, IBoss? y) => (x, y) switch
    {
        (null, null) => 0,
        (null, _) => -1,
        (_, null) => 1,
        var (boss1, boss2) => boss1.Config.Order.CompareTo(boss2.Config.Order),
    };
}