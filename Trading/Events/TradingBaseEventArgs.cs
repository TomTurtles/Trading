namespace Trading;

public class TradingBaseEventArgs(Candle candle) : EventArgs()
{
    public Candle Candle { get; } = candle;
}
