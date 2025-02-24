namespace Marek.Trading.Backtesting;

public class OnBacktestingOrderExecutedEventArgs(DateTime timestamp, Order order) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public Order Order { get; } = order;
}
