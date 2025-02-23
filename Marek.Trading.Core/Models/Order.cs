namespace Marek.Trading.Core;

public class Order(OrderSide side, string symbol)
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Symbol { get; set; } = symbol;

    [ConvertStringEnum]
    public OrderSide Side { get; set; } = side;

    [ConvertStringEnum]
    public OrderType Type => Price is null ? OrderType.Market : OrderType.Limit;

    /// <summary>
    /// Wenn Order als MarketOrder ausgeführt werden soll, dann Price NICHT setzen.
    /// Wenn Order als LimitOrder ausgeführt werden soll, dann Price setzen.
    /// </summary>
    public double? Price { get; set; }

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

    public double Quantity { get; set; } = 1;
    public double Lever { get; set; } = 1;
    public double? StopLossPrice { get; set; }
    public double? TakeProfitPrice { get; set; }
    public double? PlacedPrice { get; set; }
    public double? ExecutedPrice { get; set; }

    /// <summary>
    /// Gesamt: ExecutedPrice * Quantity * FeeRate
    /// </summary>
    public double? ExecutedFee { get; set; }

    /// <summary>
    /// Gesamt-Wert einer ausgeführten Order (OHNE Gebühren)
    /// </summary>
    public double? ExecutedValue => (ExecutedPrice is null ? null : ExecutedPrice.Value) * Quantity;
    public DateTime? PlacedTime { get; set; }
    public DateTime? ExecutedTime { get; set; }
    public DateTime? CancelledTime { get; set; }

    public static Order CreateLong(string symbol, double? lever = null)
    {
        return new Order(OrderSide.Buy, symbol)
        {
            Lever = lever ?? 1
        };
    }
    public static Order CreateShort(string symbol, double? lever = null)
    {
        return new Order(OrderSide.Sell, symbol)
        {
            Lever = lever ?? 1
        };
    }

    public override string ToString()
    {
        return $"{Side}: {ExecutedValue ?? Price} x {Quantity} = {(ExecutedValue ?? Price) * Quantity}, Lever = {Lever}";
    }
}
