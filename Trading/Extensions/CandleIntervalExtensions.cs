namespace Trading;

public static class CandleIntervalExtensions
{
    public static CandleInterval ToLonger(this CandleInterval interval, int addToPosition)
    {
        IList list = Enum.GetValues(typeof(CandleInterval));
        var elementIndex = list.IndexOf(interval);
        var newIndex = Math.Min(list.Count, elementIndex + addToPosition);
        return (CandleInterval)list[newIndex];
    }
}