﻿namespace Trading;
internal class OnGoShortEventArgs(Candle candle, Order order, double marketPrice) : TradingBaseEventArgs(candle)
{
    public Order Order { get; } = order;
    public double MarketPrice { get; } = marketPrice;
}
