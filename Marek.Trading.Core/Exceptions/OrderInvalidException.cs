namespace Marek.Trading.Core;

public class OrderInvalidException(Order order, string reason) : Exception(reason)
{
    public Order Order { get; } = order;
}
