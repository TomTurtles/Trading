namespace Marek.Trading.Core;

public class Order : IOrder
{
    public string Id { get; set; }
    public string Symbol { get; set; }

    [ConvertStringEnum]
    public OrderSide Side { get; set; }

    [ConvertStringEnum]
    public OrderType Type { get; set; }

    /// <summary>
    /// Wenn Order als MarketOrder ausgeführt werden soll, dann Price NICHT setzen.
    /// Wenn Order als LimitOrder ausgeführt werden soll, dann Price setzen.
    /// </summary>
    public double? Price { get; set; }

    [ConvertStringEnum]
    public OrderStatus Status { get; set; }
    public double Quantity { get; set; }
    public double Lever { get; set; } 
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
    public double? ExecutedValue { get; set; }
    public DateTime? PlacedTime { get; set; }
    public DateTime? ExecutedTime { get; set; }
    public DateTime? CancelledTime { get; set; }

    public override string ToString()
    {
        return $"{Side}: {ExecutedValue ?? Price} x {Quantity} = {(ExecutedValue ?? Price) * Quantity}, Lever = {Lever}";
    }
}
