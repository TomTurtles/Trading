namespace Trading;

public static class OrderExtensions
{
    public static void SetExecuted(this Order order, DateTime executionTime, decimal executionPrice, decimal feeRate)
    {
        order.ExecutedPrice = executionPrice;
        order.ExecutedTime = executionTime;
        order.ExecutedFee = executionPrice * feeRate;
        order.Status = OrderStatus.Filled;
    }

    public static bool IsOpen(this Order order) => order.ExecutedPrice is not null && order.ExecutedTime is not null;
    public static PositionSide ToPositionSide(this Order order) => order.Side switch
    {
        OrderSide.Buy => PositionSide.Long,
        OrderSide.Sell => PositionSide.Short,
        _ => throw new NotImplementedException()
    };

    public static bool IsSameSideAs(this Order order, Position position) => order.ToPositionSide() == position.Side;

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
        else if (order.Type == OrderType.Stop)
        {
            // Stop Buy: Preis muss >= Stop-Preis sein
            if (order.Side == OrderSide.Buy && candle.High >= order.Price)
            {
                return true;
            }
            // Stop Sell: Preis muss <= Stop-Preis sein
            else if (order.Side == OrderSide.Sell && candle.Low <= order.Price)
            {
                return true;
            }
        }
        return false;
    }
}
