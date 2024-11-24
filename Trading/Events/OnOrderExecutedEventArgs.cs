namespace Trading;

public class OnOrderExecutedEventArgs(Candle candle, Order order) : TradingBaseEventArgs(candle)
{
    public Order Order { get; } = order;
}
