﻿namespace Trading;
internal class OnGoLongEventArgs(Candle candle, Order order, decimal marketPrice) : TradingBaseEventArgs(candle)
{
    public Order Order { get; } = order;
    public decimal MarketPrice { get; } = marketPrice;
}