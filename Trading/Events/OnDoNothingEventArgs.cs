namespace Trading;
public class OnDoNothingEventArgs(Candle candle, string? reason = null) : TradingBaseEventArgs(candle)
{
    public string? Reason { get; } = reason;
}
