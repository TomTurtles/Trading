namespace Marek.Trading.Core;

public static class OrderExtensions
{
    public static double GetValue(this Order order)
    {
        var quantity = order.Quantity;

        double? price = null;
        if (order.IsCancelled()) price = 0;
        if (order.IsInitialized()) price = order.Price;
        if (order.IsPending()) price = order.PlacedPrice;
        if (order.IsFilled()) price = order.ExecutedPrice;
        
        return quantity * price!.Value;
    }

    public static void SetPlaced(this Order order, DateTime placedTime, double placedPrice)
    {
        if (order.IsCancelled()) throw new InvalidOperationException("cancelled orders can not be placed");
        if (order.IsFilled()) throw new InvalidOperationException("filled orders can not be placed");
        order.PlacedPrice = placedPrice;
        order.PlacedTime = placedTime;
    }

    public static void SetExecuted(this Order order, DateTime executionTime, double executionPrice, double feeRate)
    {
        if (order.IsCancelled()) throw new InvalidOperationException("cancelled orders can not be executed");
        if (order.IsFilled()) throw new InvalidOperationException("filled orders can not be executed");
        if (executionPrice <= 0) throw new InvalidOperationException($"invalid execution price '{executionPrice}'");
        order.ExecutedPrice = executionPrice;
        order.ExecutedTime = executionTime;
        order.ExecutedFee = order.Quantity * executionPrice * feeRate;
    }

    public static void SetCancelled(this Order order, DateTime cancelledTime)
    {
        if (order.IsFilled()) throw new InvalidOperationException("filled orders can not be cancelled");
        order.CancelledTime = cancelledTime;
    }

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
        if (order.Type == OrderType.Limit)
        {
            // Limit Buy: Preis muss <= Limit-Preis sein
            if (order.Side == OrderSide.Buy && candle.Low <= order.Price)
            {
                return true;
            }
            // Limit Sell: Preis muss >= Limit-Preis sein
            else if (order.Side == OrderSide.Sell && candle.High >= order.Price)
            {
                return true;
            }
        }
        return false;
    }
}
