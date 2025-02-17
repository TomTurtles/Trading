namespace Marek.Trading.Core;

public class OnOrderUpdatedEventArgs(OrderUpdateType type)
{
    public OrderUpdateType Type { get; } = type;

    public string Id { get; set; }  
    public string Symbol { get;set; }
    public OrderSide Side { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public double Quantity { get; set; }
    public double Price { get; set; } 
    public DateTime Timestamp { get; set; }
}
