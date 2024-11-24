namespace Trading;

public class Order 
{
    public Order()
    {
        ID = Guid.NewGuid().ToString();    
    }

    public string ID { get; }
    public string Symbol { get; set; }

    [ConvertStringEnum]
    public OrderSide Side { get; set; }

    [ConvertStringEnum]
    public OrderType Type { get; set; }

    [ConvertStringEnum]
    public OrderStatus Status { get; set; }
    public double Quantity { get; set; }
    public double? Price { get; set; } 
    public double? StopPrice { get; set; }
    public double? TakePrice { get; set; }
    public double? ExecutedPrice { get; set; }
    public double? ExecutedFee { get; set; }
    public DateTime? ExecutedTime { get; set; }


    public static Order CreateLong(string symbol)
    {
        return new Order()
        {
            Side = OrderSide.Buy,
            Symbol = symbol,
            Status = OrderStatus.Pending,
        };
    }

    public static Order CreateShort(string symbol)
    {
        return new Order()
        {
            Side = OrderSide.Sell,
            Symbol = symbol,
            Status = OrderStatus.Pending,
        };
    }

    public static Order Create(string symbol, OrderSide side)
    {
        return new Order()
        {
            Side = side,
            Symbol = symbol,
            Status = OrderStatus.Pending,
        };
    }
}
