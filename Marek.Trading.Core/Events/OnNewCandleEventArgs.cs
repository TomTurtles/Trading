namespace Marek.Trading.Core;

public class OnNewCandleEventArgs(Candle candle) : EventArgs
{
    public Candle Candle { get; } = candle;
}
