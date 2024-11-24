namespace Trading;

public class OnOrderPlacedEventArgs(Candle candle, Order order) : TradingBaseEventArgs(candle)
{
    public Order Order { get; set; } = order;
}
