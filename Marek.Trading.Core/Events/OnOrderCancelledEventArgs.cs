namespace Marek.Trading.Core;

public class OnOrderCancelledEventArgs(Candle candle, Order order) : TradingBaseEventArgs(candle)
{
    public Order Order { get; } = order;
}
