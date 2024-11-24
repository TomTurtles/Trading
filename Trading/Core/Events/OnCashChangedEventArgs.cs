namespace Trading;

public class OnCashChangedEventArgs(Candle candle, decimal absolute) : TradingBaseEventArgs(candle)
{
    public decimal Absolute { get; } = absolute;
}
