namespace Marek.Trading.Core;

internal class DateTimeComparer : IComparer<DateTime>
{
    private DateTimeComparer()
    {
            
    }

    public static DateTimeComparer Use() => new DateTimeComparer();
    public int Compare(DateTime x, DateTime y)
    {
        // Null-Checks für x und y
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Vergleich der DateTime-Werte
        if (x.Ticks == y.Ticks) return 0;
        return x.Ticks > y.Ticks ? 1 : -1;
    }
}
public class DateTimeEqualityComparer : IEqualityComparer<DateTime>
{
    public static DateTimeEqualityComparer Use() => new DateTimeEqualityComparer();
    public bool Equals(DateTime x, DateTime y) => GetHashCode(x) == GetHashCode(y);
    public int GetHashCode(DateTime obj) => obj.GetHashCode();
}
