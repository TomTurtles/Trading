namespace Marek.Trading.Backtesting;

public class OrderInvalidException(Order order, string reason) : Exception(reason)
{
    public Order Order { get; } = order;
}
