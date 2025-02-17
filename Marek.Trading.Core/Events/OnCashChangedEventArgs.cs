namespace Marek.Trading.Core;

public class OnCashChangedEventArgs(Candle candle, double absolute) : TradingBaseEventArgs(candle)
{
    public double Absolute { get; } = absolute;
}
