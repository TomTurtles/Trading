namespace Marek.Trading.Backtesting;

public static class BacktestingOrderExtensions
{
    public static void ValidateBeforePlacement(this Order order, double? marketPrice = null)
    {
        if (order.IsMarket())
        {
            if (marketPrice == null) throw new ArgumentNullException(nameof(marketPrice));
            
            if (order.IsLong())
            {
                // Validate TakeProfitPrice
                if (order.TakeProfitPrice is not null)
                {
                    if (order.TakeProfitPrice.Value <= marketPrice) throw new OrderInvalidException(order, "take profit price must be higher than market price");
                }
                // Validate TakeProfitPrice
                if (order.StopLossPrice is not null)
                {
                    if (order.StopLossPrice.Value >= marketPrice) throw new OrderInvalidException(order, "stop loss price must be lower than market price");
                }
            }

            if (order.IsShort())
            {
                // Validate TakeProfitPrice
                if (order.TakeProfitPrice is not null)
                {
                    if (order.TakeProfitPrice.Value >= marketPrice) throw new OrderInvalidException(order, "take profit price must be lower than market price");
                }
                // Validate TakeProfitPrice
                if (order.StopLossPrice is not null)
                {
                    if (order.StopLossPrice.Value <= marketPrice) throw new OrderInvalidException(order, "stop loss price must be higher than market price");
                }
            }
        }
        
        if (order.IsLimit())
        {
            if (order.Price == null) throw new OrderInvalidException(order, "limit order must have price set");

            if (order.IsLong())
            {
                // Validate TakeProfitPrice
                if (order.TakeProfitPrice is not null)
                {
                    if (order.TakeProfitPrice.Value <= order.Price) throw new OrderInvalidException(order, "take profit price must be higher than limit order price");
                }
                // Validate TakeProfitPrice
                if (order.StopLossPrice is not null)
                {
                    if (order.StopLossPrice.Value >= order.Price) throw new OrderInvalidException(order, "stop loss price must be lower than limit order price");
                }
            }

            if (order.IsShort())
            {
                // Validate TakeProfitPrice
                if (order.TakeProfitPrice is not null)
                {
                    if (order.TakeProfitPrice.Value >= order.Price) throw new OrderInvalidException(order, "take profit price must be lower than limit order price");
                }
                // Validate TakeProfitPrice
                if (order.StopLossPrice is not null)
                {
                    if (order.StopLossPrice.Value <= order.Price) throw new OrderInvalidException(order, "stop loss price must be higher than limit order price");
                }
            }
        }
    }

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
}
