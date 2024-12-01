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
    public double Lever { get; set; }
    public double? Price { get; set; } 
    public double? StopPrice { get; set; }
    public double? TakePrice { get; set; }
    public double? ExecutedPrice { get; set; }
    public double? ExecutedFee { get; set; }
    public DateTime? ExecutedTime { get; set; }


    public static Order CreateLong(string symbol, double? lever = null) => Create(symbol, OrderSide.Buy, lever);
    public static Order CreateShort(string symbol, double? lever = null) => Create(symbol, OrderSide.Sell, lever);
    public static Order Create(string symbol, OrderSide side, double? lever = null)
    {
        return new Order()
        {
            Side = side,
            Symbol = symbol,
            Lever = lever ?? 1,
            Status = OrderStatus.Pending,
        };
    }

    public override string ToString()
    {
        return $"{Side}: {Price} x {Quantity} = {Price * Quantity}, Lever = {Lever}";
    }
}
