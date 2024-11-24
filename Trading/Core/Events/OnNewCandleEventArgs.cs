namespace Trading;

public class OnNewCandleEventArgs(Candle candle, string symbol) : EventArgs
{
    public Candle Candle { get; } = candle;
    public string Symbol { get; } = symbol;
}
