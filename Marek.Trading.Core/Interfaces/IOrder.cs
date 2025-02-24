namespace Marek.Trading.Core;

public interface IOrder
{
    public string Id { get; }
    public string Symbol { get; }
    public OrderSide Side { get; }
    public OrderType Type { get; }
    public double? Price { get; set; }
    public OrderStatus Status { get; }
    public double Quantity { get; set; }
    public double Lever { get; set; }
    public double? StopLossPrice { get; set; }
    public double? TakeProfitPrice { get; set; }
    public double? PlacedPrice { get; }
    public double? ExecutedPrice { get; }
    public double? ExecutedFee { get; }
    public double? ExecutedValue { get; }
    public DateTime? PlacedTime { get; }
    public DateTime? ExecutedTime { get; }
    public DateTime? CancelledTime { get; }
}
