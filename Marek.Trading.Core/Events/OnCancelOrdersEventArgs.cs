namespace Marek.Trading.Core;

public class OnCancelOrdersEventArgs(Candle candle, IEnumerable<Order> orders) : TradingBaseEventArgs(candle)
{
    public IEnumerable<Order> Orders { get; } = orders;
}
