namespace Trading;
public class OnMarginCallEventArgs(Candle candle, double marketPrice) : TradingBaseEventArgs(candle)
{
    public double MarketPrice { get; } = marketPrice;
}
