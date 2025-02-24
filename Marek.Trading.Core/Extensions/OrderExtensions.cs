namespace Marek.Trading.Core;

public static class OrderExtensions
{
    public static bool IsInitialized(this Order order) => order.Status == OrderStatus.Initialized;
    public static bool IsPending(this Order order) => order.Status == OrderStatus.Pending;
    public static bool IsFilled(this Order order) => order.Status == OrderStatus.Filled;
    public static bool IsCancelled(this Order order) => order.Status == OrderStatus.Cancelled;
    public static bool IsMarket(this Order order) => order.Type == OrderType.Market;
    public static bool IsLimit(this Order order) => order.Type == OrderType.Limit;
    public static bool IsLong(this Order order) => order.Side == OrderSide.Buy;
    public static bool IsShort(this Order order) => order.Side == OrderSide.Sell;

    public static PositionSide ToPositionSide(this Order order) => order.Side switch
    {
        OrderSide.Buy => PositionSide.LONG,
        OrderSide.Sell => PositionSide.SHORT,
        _ => throw new NotImplementedException()
    };

    public static bool IsSameSideAs(this Order order, Position position) => order.ToPositionSide() == position.Side;
    public static bool IsOppositeSideAs(this Order order, Position position) => order.ToPositionSide() != position.Side;

    public static bool CandleHit(this Order order, Candle candle)
    {
        if (order.Type != OrderType.Limit) return false;

        // Limit Buy: Preis muss <= Limit-Preis sein
        if (order.Side == OrderSide.Buy && candle.Low <= order.Price)
        {
            return true;
        }

        // Limit Sell: Preis muss >= Limit-Preis sein
        if (order.Side == OrderSide.Sell && candle.High >= order.Price)
        {
            return true;
        }

        return false;
    }
}
