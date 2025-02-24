namespace Marek.Trading.Core;

public static class OrderExtensions
{
    public static bool IsInitialized(this IOrder order) => order.Status == OrderStatus.Initialized;
    public static bool IsPending(this IOrder order) => order.Status == OrderStatus.Pending;
    public static bool IsFilled(this IOrder order) => order.Status == OrderStatus.Filled;
    public static bool IsCancelled(this IOrder order) => order.Status == OrderStatus.Cancelled;
    public static bool IsMarket(this IOrder order) => order.Type == OrderType.Market;
    public static bool IsLimit(this IOrder order) => order.Type == OrderType.Limit;
    public static bool IsLong(this IOrder order) => order.Side == OrderSide.Buy;
    public static bool IsShort(this IOrder order) => order.Side == OrderSide.Sell;

    public static PositionSide ToPositionSide(this IOrder order) => order.Side switch
    {
        OrderSide.Buy => PositionSide.LONG,
        OrderSide.Sell => PositionSide.SHORT,
        _ => throw new NotImplementedException($"unknown side: {order.Side}")
    };

    public static bool IsSameSideAs(this IOrder order, IPosition position) => order.ToPositionSide() == position.Side;
    public static bool IsOppositeSideAs(this IOrder order, IPosition position) => order.ToPositionSide() != position.Side;

    public static bool CandleHit(this IOrder order, Candle candle)
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


    public static double GetValue(this IOrder order)
    {
        var quantity = order.Quantity;

        double? price = null;
        if (order.IsCancelled()) price = 0;
        if (order.IsInitialized()) price = order.Price;
        if (order.IsPending()) price = order.PlacedPrice;
        if (order.IsFilled()) price = order.ExecutedPrice;

        return quantity * price!.Value;
    }
}
