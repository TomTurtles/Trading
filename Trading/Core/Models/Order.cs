namespace Trading;

public class Order 
{
    public Order(string symbol, OrderSide side) 
    { 
        Symbol = symbol; 
        Side = side; 
    }

    public string ID { get; } = Guid.NewGuid().ToString();
    public string Symbol { get; }

    [ConvertStringEnum]
    public OrderSide Side { get; }


    [ConvertStringEnum]
    public OrderType Type { get; set; } = OrderType.Market;
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; } 
    public decimal? StopPrice { get; set; }
    public decimal? TakePrice { get; set; }
    public decimal? ExecutedPrice { get; set; }
    public decimal? ExecutedFee { get; set; }
    public DateTime? ExecutedTime { get; set; }

    [ConvertStringEnum]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public bool IsPending => Status == OrderStatus.Pending;
    public bool IsCancelled => Status == OrderStatus.Cancelled;
    public bool IsExecuted => Status == OrderStatus.Filled;
}
