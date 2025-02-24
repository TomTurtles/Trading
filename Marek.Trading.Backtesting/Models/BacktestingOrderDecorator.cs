namespace Marek.Trading.Backtesting;

public class BacktestingOrderDecorator(IOrder order) : IBacktestingOrder
{
    private readonly IOrder _order = order;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = order.Symbol;
    public OrderSide Side { get; set; } = order.Side;
    public OrderType Type => Price is null ? OrderType.Market : OrderType.Limit;

    /// <summary>
    /// Wenn Order als MarketOrder ausgeführt werden soll, dann Price NICHT setzen.
    /// Wenn Order als LimitOrder ausgeführt werden soll, dann Price setzen.
    /// </summary>
    public double? Price { get; set; } = order.Price;

    [ConvertStringEnum]
    public OrderStatus Status
    {
        get
        {
            if (CancelledTime.HasValue) return OrderStatus.Cancelled;
            if (ExecutedTime.HasValue) return OrderStatus.Filled;
            if (PlacedTime.HasValue) return OrderStatus.Pending;
            return OrderStatus.Initialized;
        }
    }

    public double Quantity { get; set; } = order.Quantity;
    public double Lever { get; set; } = order.Lever;
    public double? StopLossPrice { get; set; } = order.StopLossPrice;
    public double? TakeProfitPrice { get; set; } = order.TakeProfitPrice;
    public double? PlacedPrice { get; set; } = order.PlacedPrice;
    public double? ExecutedPrice { get; set; } = order.ExecutedPrice;

    /// <summary>
    /// Gesamt: ExecutedPrice * Quantity * FeeRate
    /// </summary>
    public double? ExecutedFee { get; set; } = order.ExecutedFee;

    /// <summary>
    /// Gesamt-Wert einer ausgeführten Order (OHNE Gebühren)
    /// </summary>
    public double? ExecutedValue => (ExecutedPrice is null ? null : ExecutedPrice.Value) * Quantity;
    public DateTime? PlacedTime { get; set; } = order.PlacedTime;

    public DateTime? ExecutedTime { get; set; } = order.ExecutedTime;

    public DateTime? CancelledTime { get; set; } = order.CancelledTime;



    public void ValidateBeforePlacement(double? marketPrice = null)
    {
        if (this.IsMarket())
        {
            if (marketPrice == null) throw new ArgumentNullException(nameof(marketPrice));

            if (this.IsLong())
            {
                // Validate TakeProfitPrice
                if (TakeProfitPrice is not null)
                {
                    if (TakeProfitPrice.Value <= marketPrice) throw new OrderInvalidException(this, "take profit price must be higher than market price");
                }
                // Validate TakeProfitPrice
                if (StopLossPrice is not null)
                {
                    if (StopLossPrice.Value >= marketPrice) throw new OrderInvalidException(this, "stop loss price must be lower than market price");
                }
            }

            if (this.IsShort())
            {
                // Validate TakeProfitPrice
                if (TakeProfitPrice is not null)
                {
                    if (TakeProfitPrice.Value >= marketPrice) throw new OrderInvalidException(this, "take profit price must be lower than market price");
                }
                // Validate TakeProfitPrice
                if (StopLossPrice is not null)
                {
                    if (StopLossPrice.Value <= marketPrice) throw new OrderInvalidException(this, "stop loss price must be higher than market price");
                }
            }
        }

        if (this.IsLimit())
        {
            if (Price == null) throw new OrderInvalidException(this, "limit order must have price set");

            if (this.IsLong())
            {
                // Validate TakeProfitPrice
                if (TakeProfitPrice is not null)
                {
                    if (TakeProfitPrice.Value <= Price) throw new OrderInvalidException(this, "take profit price must be higher than limit order price");
                }
                // Validate TakeProfitPrice
                if (StopLossPrice is not null)
                {
                    if (StopLossPrice.Value >= Price) throw new OrderInvalidException(this, "stop loss price must be lower than limit order price");
                }
            }

            if (this.IsShort())
            {
                // Validate TakeProfitPrice
                if (TakeProfitPrice is not null)
                {
                    if (TakeProfitPrice.Value >= Price) throw new OrderInvalidException(this, "take profit price must be lower than limit order price");
                }
                // Validate TakeProfitPrice
                if (StopLossPrice is not null)
                {
                    if (StopLossPrice.Value <= Price) throw new OrderInvalidException(this, "stop loss price must be higher than limit order price");
                }
            }
        }
    }


    public void SetPlaced(DateTime placedTime, double placedPrice)
    {
        if (this.IsCancelled()) throw new InvalidOperationException("cancelled orders can not be placed");
        if (this.IsFilled()) throw new InvalidOperationException("filled orders can not be placed");
        PlacedPrice = placedPrice;
        PlacedTime = placedTime;
    }

    public void SetExecuted(DateTime executionTime, double executionPrice, double feeRate)
    {
        if (this.IsCancelled()) throw new InvalidOperationException("cancelled orders can not be executed");
        if (this.IsFilled()) throw new InvalidOperationException("filled orders can not be executed");
        if (executionPrice <= 0) throw new InvalidOperationException($"invalid execution price '{executionPrice}'");
        ExecutedPrice = executionPrice;
        ExecutedTime = executionTime;
        ExecutedFee = order.Quantity * executionPrice * feeRate;
    }

    public void SetCancelled(DateTime cancelledTime)
    {
        if (this.IsFilled()) throw new InvalidOperationException("filled orders can not be cancelled");
        CancelledTime = cancelledTime;
    }

    public override string ToString()
    {
        return $"{Side}: {ExecutedValue ?? Price} x {Quantity} = {(ExecutedValue ?? Price) * Quantity}, Lever = {Lever}";
    }
}
