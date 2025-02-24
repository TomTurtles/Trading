namespace Marek.Trading.Core;
public class OrderBuilder
{
    public static IOrder CreateLong(string symbol, double lever = 1)
    {
        return new Order
        {
            Side = OrderSide.Buy,
            Symbol = symbol,
            Lever = lever,
        };
    }

    public static IOrder CreateShort(string symbol, double lever = 1)
    {
        return new Order
        {
            Side = OrderSide.Sell,
            Symbol = symbol,
            Lever = lever,
        };
    }

}
