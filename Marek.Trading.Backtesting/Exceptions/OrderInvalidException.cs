namespace Marek.Trading.Backtesting;

public class OrderInvalidException(IOrder order, string reason) : Exception(reason)
{
    public IOrder Order { get; } = order;
}
