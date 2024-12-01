namespace Trading;

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
        return x.CompareTo(y);
    }
}
public class DateTimeEqualityComparer : IEqualityComparer<DateTime>
{
    public static DateTimeEqualityComparer Use() => new DateTimeEqualityComparer();
    public bool Equals(DateTime x, DateTime y) => GetHashCode(x) == GetHashCode(y);
    public int GetHashCode(DateTime obj) => obj.GetHashCode();
}
