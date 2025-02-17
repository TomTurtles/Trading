namespace Marek.Trading.Core;

public class TradingBaseEventArgs(Candle candle) : EventArgs()
{
    public Candle Candle { get; } = candle;
}
