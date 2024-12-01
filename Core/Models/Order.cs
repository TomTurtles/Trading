namespace Trading;

public class Order
{
    public enum OrderType { Market, Limit, Stop, StopLimit }
    public enum OrderSide { Buy, Sell }
    public enum OrderStatus { Pending, Filled, Cancelled }

    public Guid OrderId { get; set; }
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal? ExecutedPrice { get; set; }
    public DateTime? ExecutedTime { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Symbol { get; set; }
}
