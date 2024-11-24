namespace Trading;
internal class OnGoLongEventArgs(Candle candle, Order order, double marketPrice) : TradingBaseEventArgs(candle)
{
    public Order Order { get; } = order;
    public double MarketPrice { get; } = marketPrice;
}
