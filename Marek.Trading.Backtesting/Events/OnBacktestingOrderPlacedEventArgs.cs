namespace Marek.Trading.Backtesting;

public class OnBacktestingOrderPlacedEventArgs(DateTime timestamp, Order order) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public Order Order { get; } = order;
}
