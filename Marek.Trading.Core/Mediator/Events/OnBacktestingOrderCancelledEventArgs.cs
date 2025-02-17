namespace Marek.Trading.Core;

public class OnBacktestingOrderCancelledEventArgs(DateTime timestamp, Order order) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public Order Order { get; } = order;
}
