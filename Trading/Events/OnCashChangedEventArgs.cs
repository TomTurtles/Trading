namespace Trading;

public class OnCashChangedEventArgs(Candle candle, double absolute) : TradingBaseEventArgs(candle)
{
    public double Absolute { get; } = absolute;
}
