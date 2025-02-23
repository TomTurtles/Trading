namespace Marek.Trading.Core;

public class OnCandlesLoadedEventArgs(List<Candle> candles) : EventArgs
{
    public List<Candle> Candles { get; } = candles;
}
