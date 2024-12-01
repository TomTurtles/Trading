namespace Trading;

public static class StringExtensions
{
    public static Position.PositionSide ToPositionSide(this string str)
    {
        return str.ToUpperInvariant() switch
        {
            "BUY" => Position.PositionSide.Long,
            "SELL" => Position.PositionSide.Short,
            _ => throw new InvalidOperationException(str),
        };
    }
}
