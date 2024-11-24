namespace Trading;
public class OnMarginCallEventArgs(Candle candle, decimal marketPrice) : TradingBaseEventArgs(candle)
{
    public decimal MarketPrice { get; } = marketPrice;
}
